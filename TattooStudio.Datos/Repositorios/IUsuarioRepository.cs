using TattooStudio.Entidades;

namespace TattooStudio.Datos.Repositorios;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task AgregarAsync(Usuario usuario);
}
