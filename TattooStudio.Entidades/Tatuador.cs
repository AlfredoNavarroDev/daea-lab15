using System;
using System.Collections.Generic;

namespace TattooStudio.Entidades;

public partial class Tatuador
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Especialidad { get; set; } = null!;

    public int AniosExperiencia { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<CitaTatuaje> CitaTatuajes { get; set; } = new List<CitaTatuaje>();
}
