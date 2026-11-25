using Microsoft.EntityFrameworkCore;
using TattooStudio.Datos.Data;
using TattooStudio.Entidades;

namespace TattooStudio.Datos.Repositorios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly TattooStudioContext _context;

    public UsuarioRepository(TattooStudioContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        return await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Correo == correo);
    }

    public async Task AgregarAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }
}
