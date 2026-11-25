using TattooStudio.Entidades.Dtos;

namespace TattooStudio.Negocio.Servicios;

public interface ICitaTatuajeService
{
    Task<List<CitaTatuajeDto>> ObtenerTodosAsync(int? tatuadorId, string? estado);
    Task<List<CitaTatuajeDto>> ObtenerPorTatuadorAsync(int tatuadorId);
    Task<CitaTatuajeDto?> ObtenerPorIdAsync(int id);
    Task<CitaTatuajeDto?> CrearAsync(CitaTatuajeCrearDto dto);
    Task<bool?> ActualizarAsync(int id, CitaTatuajeActualizarDto dto);
    Task<bool> EliminarAsync(int id);
}
