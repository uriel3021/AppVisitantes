using System;
using System.Collections.Generic;

namespace KYCApp.Models;

public partial class Visitante
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string ApellidoMaterno { get; set; } = null!;

    public string CorreoElectronico { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public DateTime FechaVisita { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<CodigoQr> CodigoQrs { get; set; } = new List<CodigoQr>();
}
