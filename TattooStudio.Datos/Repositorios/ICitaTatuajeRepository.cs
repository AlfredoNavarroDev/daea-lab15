using TattooStudio.Entidades;

namespace TattooStudio.Datos.Repositorios;

public interface ICitaTatuajeRepository
{
    Task<List<CitaTatuaje>> ObtenerTodosAsync(int? tatuadorId, string? estado);
    Task<List<CitaTatuaje>> ObtenerPorTatuadorAsync(int tatuadorId);
    Task<CitaTatuaje?> ObtenerPorIdAsync(int id);
    Task<CitaTatuaje?> ObtenerPorIdConTatuadorAsync(int id);
    Task<List<CitaTatuaje>> ObtenerActivasPorTatuadorAsync(int tatuadorId, int? excluirCitaId);
    Task<CitaTatuaje> CrearAsync(CitaTatuaje cita);
    Task ActualizarAsync(CitaTatuaje cita);
    Task EliminarAsync(CitaTatuaje cita);
}
