using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TattooStudio.Entidades.Dtos;
using TattooStudio.Negocio.Excepciones;
using TattooStudio.Negocio.Servicios;

namespace TattooStudioApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAutenticacionService _autenticacionService;

    public AuthController(IAutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    // POST api/auth/registro
    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            await _autenticacionService.RegistrarAsync(dto);
            return StatusCode(StatusCodes.Status201Created);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    // POST api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var token = await _autenticacionService.LoginAsync(dto);
        if (token is null)
        {
            return Unauthorized();
        }

        return Ok(token);
    }

    // GET api/auth/perfil
    [HttpGet("perfil")]
    [Authorize]
    public ActionResult<PerfilDto> Perfil()
    {
        var perfil = new PerfilDto
        {
            Correo = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            Rol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
        };

        return Ok(perfil);
    }
}
