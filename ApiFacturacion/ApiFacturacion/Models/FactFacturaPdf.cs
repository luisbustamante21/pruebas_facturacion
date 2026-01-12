using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactFacturaPdf
{
    public int Idfactfacturapdf { get; set; }

    public int? FacturaId { get; set; }

    public byte[]? Pdf { get; set; }

    public DateTime? CreadoEn { get; set; }

    public virtual FactFactura? Factura { get; set; }
}
