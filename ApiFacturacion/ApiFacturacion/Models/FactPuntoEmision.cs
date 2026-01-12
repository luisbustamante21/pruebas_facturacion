using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactPuntoEmision
{
    public int Idfactpuntoemision { get; set; }

    public int? EstablecimientoId { get; set; }

    public string Codigo { get; set; } = null!;

    public int? SecuencialActual { get; set; }

    public virtual FactEstablecimiento? Establecimiento { get; set; }

    public virtual ICollection<FactFactura> FactFacturas { get; set; } = new List<FactFactura>();
}
