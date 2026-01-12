using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactFacturaXml
{
    public int Idfactfacturaxml { get; set; }

    public int? FacturaId { get; set; }

    public string? XmlFirmado { get; set; }

    public string? XmlAutorizado { get; set; }

    public string? NumeroAutorizacion { get; set; }

    public DateTime? FechaAutorizacion { get; set; }

    public string? EstadoAutorizacion { get; set; }

    public string? MensajeAutorizacion { get; set; }

    public DateTime? CreadoEn { get; set; }

    public virtual FactFactura? Factura { get; set; }
}
