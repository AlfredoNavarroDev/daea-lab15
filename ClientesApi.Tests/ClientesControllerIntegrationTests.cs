using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using ClientesApi.Modelos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ClientesApi.Tests;

public class ClientesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Clave = "ClientesApi-Tests-ClaveSecreta-32-Caracteres-Minimo";
    private const string Emisor = "TattooStudio.Api";
    private const string Audiencia = "TattooStudio.Clientes";

    private readonly WebApplicationFactory<Program> _factory;

    public ClientesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
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
    public async Task GetClientes_SinToken_Devuelve401()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetClientes_ConTokenValido_Devuelve200ConLista()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerarTokenDePrueba());

        var respuesta = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var clientes = await respuesta.Content.ReadFromJsonAsync<List<Cliente>>();
        Assert.NotNull(clientes);
        Assert.NotEmpty(clientes!);
    }

    [Fact]
    public async Task GetClientePorId_ConIdInexistente_Devuelve404()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerarTokenDePrueba());

        var respuesta = await client.GetAsync("/api/clientes/9999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
