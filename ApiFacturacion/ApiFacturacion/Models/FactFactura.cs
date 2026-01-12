using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactFactura
{
    public int Idfactfactura { get; set; }

    public int? PuntoEmisionId { get; set; }

    public string? FacturaId { get; set; }

    public string? Version { get; set; }

    public string? TipoEmision { get; set; }

    public string? ClaveAcceso { get; set; }

    public string? CodDoc { get; set; }

    public string? Secuencial { get; set; }

    public DateOnly? FechaEmision { get; set; }

    public string? RazonSocialComprador { get; set; }

    public string? TipoIdentificacionComprador { get; set; }

    public string? IdentificacionComprador { get; set; }

    public string? DireccionComprador { get; set; }

    public decimal? TotalSinImpuestos { get; set; }

    public decimal? TotalDescuento { get; set; }

    public decimal? Propina { get; set; }

    public decimal? ImporteTotal { get; set; }

    public string? Moneda { get; set; }

    public int? Ambiente { get; set; }

    public virtual ICollection<FactFacturaDetalle> FactFacturaDetalles { get; set; } = new List<FactFacturaDetalle>();

    public virtual ICollection<FactFacturaInfoAdicional> FactFacturaInfoAdicionals { get; set; } = new List<FactFacturaInfoAdicional>();

    public virtual ICollection<FactFacturaPago> FactFacturaPagos { get; set; } = new List<FactFacturaPago>();

    public virtual FactFacturaPdf? FactFacturaPdf { get; set; }

    public virtual ICollection<FactFacturaTotalImpuesto> FactFacturaTotalImpuestos { get; set; } = new List<FactFacturaTotalImpuesto>();

    public virtual FactFacturaXml? FactFacturaXml { get; set; }

    public virtual FactPuntoEmision? PuntoEmision { get; set; }
}
