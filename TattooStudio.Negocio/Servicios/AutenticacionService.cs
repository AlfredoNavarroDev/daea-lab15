using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TattooStudio.Datos.Repositorios;
using TattooStudio.Entidades;
using TattooStudio.Entidades.Dtos;
using TattooStudio.Negocio.Excepciones;

namespace TattooStudio.Negocio.Servicios;

public class AutenticacionService : IAutenticacionService
{
    private const string RolPorDefecto = "Recepcionista";

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AutenticacionService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    public async Task RegistrarAsync(RegistroDto dto)
    {
        var existente = await _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo);
        if (existente is not null)
        {
            throw new ReglaNegocioException($"El correo '{dto.Correo}' ya está registrado.");
        }

        var usuario = new Usuario
        {
            Correo = dto.Correo,
            ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave),
            Rol = RolPorDefecto,
        };

        await _usuarioRepository.AgregarAsync(usuario);
    }

    public async Task<TokenDto?> LoginAsync(LoginDto dto)
    {
        var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Clave, usuario.ClaveHash))
        {
            return null;
        }

        return GenerarToken(usuario);
    }

    private TokenDto GenerarToken(Usuario usuario)
    {
        var jwtConfig = _configuration.GetSection("Jwt");
        var clave = jwtConfig["Clave"]!;
        var minutos = int.Parse(jwtConfig["MinutosExpiracion"]!);
        var expira = DateTime.UtcNow.AddMinutes(minutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol),
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Emisor"],
            audience: jwtConfig["Audiencia"],
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return new TokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expira = expira,
        };
    }
}
