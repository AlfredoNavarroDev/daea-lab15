using ClientesApi.Modelos;

namespace ClientesApi.Repositorios;

public interface IClienteRepository
{
    List<Cliente> ObtenerTodos();
    Cliente? ObtenerPorId(int id);
}
