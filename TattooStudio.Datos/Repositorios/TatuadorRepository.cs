using Microsoft.EntityFrameworkCore;
using TattooStudio.Datos.Data;
using TattooStudio.Entidades;

namespace TattooStudio.Datos.Repositorios;

public class TatuadorRepository : ITatuadorRepository
{
    private readonly TattooStudioContext _context;

    public TatuadorRepository(TattooStudioContext context)
    {
        _context = context;
    }

    public async Task<List<Tatuador>> ObtenerTodosAsync(string? especialidad, bool? activo)
    {
        IQueryable<Tatuador> query = _context.Tatuadors.AsNoTracking().Include(t => t.CitaTatuajes);

        if (!string.IsNullOrWhiteSpace(especialidad))
        {
            query = query.Where(t => t.Especialidad.Contains(especialidad));
        }

        if (activo.HasValue)
        {
            query = query.Where(t => t.Activo == activo.Value);
        }

        return await query.OrderBy(t => t.Nombre).ToListAsync();
    }

    public async Task<Tatuador?> ObtenerPorIdAsync(int id)
    {
        return await _context.Tatuadors.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tatuador?> ObtenerPorIdConCitasAsync(int id)
    {
        return await _context.Tatuadors
            .Include(t => t.CitaTatuajes)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tatuador> CrearAsync(Tatuador tatuador)
    {
        _context.Tatuadors.Add(tatuador);
        await _context.SaveChangesAsync();
        return tatuador;
    }

    public async Task ActualizarAsync(Tatuador tatuador)
    {
        _context.Tatuadors.Update(tatuador);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TieneCitasActivasAsync(int tatuadorId)
    {
        return await _context.CitaTatuajes
            .AsNoTracking()
            .AnyAsync(c => c.TatuadorId == tatuadorId && (c.Estado == "Pendiente" || c.Estado == "Confirmada"));
    }
}
