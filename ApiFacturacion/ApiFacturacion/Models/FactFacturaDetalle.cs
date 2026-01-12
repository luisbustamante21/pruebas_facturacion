using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactFacturaDetalle
{
    public int Idfactfacturadetalle { get; set; }

    public int? FacturaId { get; set; }

    public string? CodigoPrincipal { get; set; }

    public string? CodigoAuxiliar { get; set; }

    public string? Descripcion { get; set; }

    public decimal? Cantidad { get; set; }

    public decimal? PrecioUnitario { get; set; }

    public decimal? Descuento { get; set; }

    public decimal? PrecioTotalSinImpuesto { get; set; }

    public virtual FactFactura? Factura { get; set; }

    public virtual ICollection<FacturaDetalleImpuesto> FacturaDetalleImpuestos { get; set; } = new List<FacturaDetalleImpuesto>();
}
