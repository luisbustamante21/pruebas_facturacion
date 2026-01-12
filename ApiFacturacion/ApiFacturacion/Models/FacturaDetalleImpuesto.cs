using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FacturaDetalleImpuesto
{
    public int Idfacturadetalleimpuesto { get; set; }

    public int? DetalleId { get; set; }

    public string? Codigo { get; set; }

    public string? CodigoPorcentaje { get; set; }

    public decimal? Tarifa { get; set; }

    public decimal? BaseImponible { get; set; }

    public decimal? Valor { get; set; }

    public virtual FactFacturaDetalle? Detalle { get; set; }
}
