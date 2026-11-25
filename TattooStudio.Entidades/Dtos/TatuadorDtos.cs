using System.ComponentModel.DataAnnotations;

namespace TattooStudio.Entidades.Dtos;

public class TatuadorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especialidad { get; set; } = null!;
    public int AniosExperiencia { get; set; }
    public bool Activo { get; set; }
    public int CantidadCitas { get; set; }
}

public class TatuadorCrearDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Especialidad { get; set; } = null!;

    [Range(0, 60)]
    public int AniosExperiencia { get; set; }
}

public class TatuadorActualizarDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Especialidad { get; set; } = null!;

    [Range(0, 60)]
    public int AniosExperiencia { get; set; }

    public bool Activo { get; set; }
}
