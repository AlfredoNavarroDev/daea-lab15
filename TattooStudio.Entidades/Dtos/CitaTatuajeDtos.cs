using System.ComponentModel.DataAnnotations;

namespace TattooStudio.Entidades.Dtos;

public class CitaTatuajeDto
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = null!;
    public string ClienteCorreo { get; set; } = null!;
    public DateTime FechaHora { get; set; }
    public decimal EstimacionHoras { get; set; }
    public decimal SeñaPagada { get; set; }
    public string Estado { get; set; } = null!;
    public int TatuadorId { get; set; }
    public string TatuadorNombre { get; set; } = null!;
}

public class CitaTatuajeCrearDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string ClienteNombre { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string ClienteCorreo { get; set; } = null!;

    [Required]
    public DateTime FechaHora { get; set; }

    [Range(0.5, 12)]
    public decimal EstimacionHoras { get; set; }

    [Range(0, 100000)]
    public decimal SeñaPagada { get; set; }

    [Required]
    public int TatuadorId { get; set; }
}

public class CitaTatuajeActualizarDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string ClienteNombre { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string ClienteCorreo { get; set; } = null!;

    [Required]
    public DateTime FechaHora { get; set; }

    [Range(0.5, 12)]
    public decimal EstimacionHoras { get; set; }

    [Range(0, 100000)]
    public decimal SeñaPagada { get; set; }

    [Required]
    [RegularExpression("^(Pendiente|Confirmada|Cancelada)$", ErrorMessage = "Estado debe ser Pendiente, Confirmada o Cancelada.")]
    public string Estado { get; set; } = null!;

    [Required]
    public int TatuadorId { get; set; }
}
