using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GatewayApi.Tests;

public class GatewayIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GatewayIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var gatewayContentRoot = EncontrarCarpetaGatewayApi();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(gatewayContentRoot);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Clave"] = "GatewayApi-Tests-ClaveSecreta-32-Caracteres-Minimo",
                    ["Jwt:Emisor"] = "TattooStudio.Api",
                    ["Jwt:Audiencia"] = "TattooStudio.Clientes",
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

    [Fact]
    public async Task PostTatuadores_SinToken_Devuelve401_SinLlegarAlDownstream()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.PostAsJsonAsync("/gateway/tatuadores", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetTatuadores_RutaPublica_SinDownstreamCorriendo_Devuelve502()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/gateway/tatuadores");

        Assert.Equal(HttpStatusCode.BadGateway, respuesta.StatusCode);
    }
}
