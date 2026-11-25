using ClientesApi.Modelos;

namespace ClientesApi.Repositorios;

public class ClienteRepository : IClienteRepository
{
    private readonly List<Cliente> _clientes = new()
    {
        new Cliente { Id = 1, Nombre = "Lucia Fernandez", Correo = "lucia.fernandez@example.com", Telefono = "987654321" },
        new Cliente { Id = 2, Nombre = "Marco Rivas", Correo = "marco.rivas@example.com", Telefono = "987654322" },
        new Cliente { Id = 3, Nombre = "Elena Quispe", Correo = "elena.quispe@example.com", Telefono = "987654323" },
        new Cliente { Id = 4, Nombre = "Diego Salazar", Correo = "diego.salazar@example.com", Telefono = "987654324" },
        new Cliente { Id = 5, Nombre = "Valeria Nunez", Correo = "valeria.nunez@example.com", Telefono = "987654325" },
    };

    public List<Cliente> ObtenerTodos() => _clientes;

    public Cliente? ObtenerPorId(int id) => _clientes.FirstOrDefault(c => c.Id == id);
}
