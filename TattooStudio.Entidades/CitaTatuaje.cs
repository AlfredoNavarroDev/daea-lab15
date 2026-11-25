using System;
using System.Collections.Generic;

namespace TattooStudio.Entidades;

public partial class CitaTatuaje
{
    public int Id { get; set; }

    public string ClienteNombre { get; set; } = null!;

    public string ClienteCorreo { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public decimal EstimacionHoras { get; set; }

    public decimal SeñaPagada { get; set; }

    public string Estado { get; set; } = null!;

    public int TatuadorId { get; set; }

    public virtual Tatuador Tatuador { get; set; } = null!;
}
