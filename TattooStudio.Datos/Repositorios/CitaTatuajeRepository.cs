using Microsoft.EntityFrameworkCore;
using TattooStudio.Datos.Data;
using TattooStudio.Entidades;

namespace TattooStudio.Datos.Repositorios;

public class CitaTatuajeRepository : ICitaTatuajeRepository
{
    private readonly TattooStudioContext _context;

    public CitaTatuajeRepository(TattooStudioContext context)
    {
        _context = context;
    }

    public async Task<List<CitaTatuaje>> ObtenerTodosAsync(int? tatuadorId, string? estado)
    {
        var query = _context.CitaTatuajes.AsNoTracking().Include(c => c.Tatuador).AsQueryable();

        if (tatuadorId.HasValue)
        {
            query = query.Where(c => c.TatuadorId == tatuadorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(c => c.Estado == estado);
        }

        return await query.OrderBy(c => c.FechaHora).ToListAsync();
    }

    public async Task<List<CitaTatuaje>> ObtenerPorTatuadorAsync(int tatuadorId)
    {
        return await _context.CitaTatuajes
            .AsNoTracking()
            .Include(c => c.Tatuador)
            .Where(c => c.TatuadorId == tatuadorId)
            .OrderBy(c => c.FechaHora)
            .ToListAsync();
    }

    public async Task<CitaTatuaje?> ObtenerPorIdAsync(int id)
    {
        return await _context.CitaTatuajes.AsNoTracking().Include(c => c.Tatuador).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CitaTatuaje?> ObtenerPorIdConTatuadorAsync(int id)
    {
        return await _context.CitaTatuajes.Include(c => c.Tatuador).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<CitaTatuaje>> ObtenerActivasPorTatuadorAsync(int tatuadorId, int? excluirCitaId)
    {
        var query = _context.CitaTatuajes
            .AsNoTracking()
            .Where(c => c.TatuadorId == tatuadorId && c.Estado != "Cancelada");

        if (excluirCitaId.HasValue)
        {
            query = query.Where(c => c.Id != excluirCitaId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<CitaTatuaje> CrearAsync(CitaTatuaje cita)
    {
        _context.CitaTatuajes.Add(cita);
        await _context.SaveChangesAsync();
        return cita;
    }

    public async Task ActualizarAsync(CitaTatuaje cita)
    {
        _context.CitaTatuajes.Update(cita);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(CitaTatuaje cita)
    {
        _context.CitaTatuajes.Remove(cita);
        await _context.SaveChangesAsync();
    }
}
