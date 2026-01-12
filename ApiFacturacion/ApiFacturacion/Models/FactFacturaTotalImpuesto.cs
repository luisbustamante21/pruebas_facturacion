using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactFacturaTotalImpuesto
{
    public int Idfactfacturatotalimpuesto { get; set; }

    public int? FacturaId { get; set; }

    public string? Codigo { get; set; }

    public string? CodigoPorcentaje { get; set; }

    public decimal? DescuentoAdicional { get; set; }

    public decimal? BaseImponible { get; set; }

    public decimal? Valor { get; set; }

    public virtual FactFactura? Factura { get; set; }
}
