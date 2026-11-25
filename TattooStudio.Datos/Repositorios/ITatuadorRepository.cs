using TattooStudio.Entidades;

namespace TattooStudio.Datos.Repositorios;

public interface ITatuadorRepository
{
    Task<List<Tatuador>> ObtenerTodosAsync(string? especialidad, bool? activo);
    Task<Tatuador?> ObtenerPorIdAsync(int id);
    Task<Tatuador?> ObtenerPorIdConCitasAsync(int id);
    Task<Tatuador> CrearAsync(Tatuador tatuador);
    Task ActualizarAsync(Tatuador tatuador);
    Task<bool> TieneCitasActivasAsync(int tatuadorId);
}
