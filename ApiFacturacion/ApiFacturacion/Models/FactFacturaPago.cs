using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactFacturaPago
{
    public int Idfactfacturapago { get; set; }

    public int? FacturaId { get; set; }

    public string? FormaPago { get; set; }

    public decimal? Total { get; set; }

    public int? Plazo { get; set; }

    public string? UnidadTiempo { get; set; }

    public virtual FactFactura? Factura { get; set; }
}
