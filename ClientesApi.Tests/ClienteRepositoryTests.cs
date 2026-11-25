using ClientesApi.Repositorios;
using Xunit;

namespace ClientesApi.Tests;

public class ClienteRepositoryTests
{
    [Fact]
    public void ObtenerTodos_DevuelveLosCincoClientesSemilla()
    {
        IClienteRepository repo = new ClienteRepository();

        var clientes = repo.ObtenerTodos();

        Assert.Equal(5, clientes.Count);
    }

    [Fact]
    public void ObtenerPorId_ConIdExistente_DevuelveElCliente()
    {
        IClienteRepository repo = new ClienteRepository();

        var cliente = repo.ObtenerPorId(1);

        Assert.NotNull(cliente);
        Assert.Equal(1, cliente!.Id);
    }

    [Fact]
    public void ObtenerPorId_ConIdInexistente_DevuelveNull()
    {
        IClienteRepository repo = new ClienteRepository();

        var cliente = repo.ObtenerPorId(9999);

        Assert.Null(cliente);
    }
}
