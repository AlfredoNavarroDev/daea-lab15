using TattooStudio.Entidades.Dtos;

namespace TattooStudio.Negocio.Servicios;

public interface IAutenticacionService
{
    Task RegistrarAsync(RegistroDto dto);
    Task<TokenDto?> LoginAsync(LoginDto dto);
}
