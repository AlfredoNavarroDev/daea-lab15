using System.ComponentModel.DataAnnotations;

namespace TattooStudio.Entidades.Dtos;

public class RegistroDto
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Correo { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Clave { get; set; } = null!;
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Correo { get; set; } = null!;

    [Required]
    public string Clave { get; set; } = null!;
}

public class TokenDto
{
    public string Token { get; set; } = null!;
    public DateTime Expira { get; set; }
}

public class PerfilDto
{
    public string Correo { get; set; } = null!;
    public string Rol { get; set; } = null!;
}
