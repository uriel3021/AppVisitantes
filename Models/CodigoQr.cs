using System;
using System.Collections.Generic;

namespace KYCApp.Models;

public partial class CodigoQr
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = null!;

    public Guid VisitanteId { get; set; }

    public virtual Visitante Visitante { get; set; } = null!;
}
