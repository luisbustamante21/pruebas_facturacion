using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactFacturaInfoAdicional
{
    public int Idfactfacturainfoadicional { get; set; }

    public int? FacturaId { get; set; }

    public string? Nombre { get; set; }

    public string? Valor { get; set; }

    public virtual FactFactura? Factura { get; set; }
}
