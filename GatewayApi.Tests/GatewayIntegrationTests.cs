using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace GatewayApi.Tests;

public class GatewayIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Clave = "GatewayApi-Tests-ClaveSecreta-32-Caracteres-Minimo";
    private const string Emisor = "TattooStudio.Api";
    private const string Audiencia = "TattooStudio.Clientes";

    private readonly WebApplicationFactory<Program> _factory;

    public GatewayIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var gatewayContentRoot = EncontrarCarpetaGatewayApi();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseContentRoot(gatewayContentRoot);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Clave"] = Clave,
                    ["Jwt:Emisor"] = Emisor,
                    ["Jwt:Audiencia"] = Audiencia,
                    ["Jwt:MinutosExpiracion"] = "60",
                });
            });
        });
    }

    private static string EncontrarCarpetaGatewayApi()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (directorio is not null && !File.Exists(Path.Combine(directorio.FullName, "GatewayApi", "ocelot.json")))
        {
            directorio = directorio.Parent;
        }

        if (directorio is null)
        {
            throw new InvalidOperationException("No se encontro GatewayApi/ocelot.json subiendo desde " + AppContext.BaseDirectory);
        }

        return Path.Combine(directorio.FullName, "GatewayApi");
    }

    private static string GenerarTokenDePrueba()
    {
        var claims = new[] { new Claim(ClaimTypes.Email, "test@example.com") };
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Clave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Emisor,
            audience: Audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task PostTatuadores_SinToken_Devuelve401_SinLlegarAlDownstream()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.PostAsJsonAsync("/gateway/tatuadores", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    // Precondicion: requiere que TattooStudio.Api NO este corriendo en local
    // (puerto 5225 libre). Si esta corriendo, el downstream real responde
    // 401 (su propio [Authorize]) en vez de que Ocelot falle al reenviar,
    // y este test se leeria como fallo sin que el codigo este mal.
    [Fact]
    public async Task GetTatuadores_RutaPublica_SinDownstreamCorriendo_Devuelve502()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/gateway/tatuadores");

        Assert.Equal(HttpStatusCode.BadGateway, respuesta.StatusCode);
    }

    // Precondicion: requiere que TattooStudio.Api NO este corriendo en local
    // (puerto 5225 libre), igual que el test anterior. Prueba que el Gateway
    // ACEPTA un token valido (misma Clave/Emisor/Audiencia de este fixture) y
    // llega a intentar el reenvio por Ocelot — 502 porque nada escucha en
    // 5225 en este entorno de test, no 401, lo que confirmaria que el
    // middleware de auth del Gateway de verdad deja pasar tokens validos.
    [Fact]
    public async Task PostTatuadores_ConTokenValido_Devuelve502_PorqueNoHayDownstream()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerarTokenDePrueba());

        var respuesta = await client.PostAsJsonAsync("/gateway/tatuadores", new { });

        Assert.Equal(HttpStatusCode.BadGateway, respuesta.StatusCode);
    }
}
