using TattooStudio.Entidades.Dtos;

namespace TattooStudio.Negocio.Servicios;

public interface ITatuadorService
{
    Task<List<TatuadorDto>> ObtenerTodosAsync(string? especialidad, bool? activo);
    Task<TatuadorDto?> ObtenerPorIdAsync(int id);
    Task<TatuadorDto> CrearAsync(TatuadorCrearDto dto);
    Task<bool> ActualizarAsync(int id, TatuadorActualizarDto dto);
    Task<bool> DesactivarAsync(int id);
}
