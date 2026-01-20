using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace ApiFacturacion.Modelos
{
    public class ValidarComprobanteRequest {
        public InfoDelCliente Info { get; set; } = new();
        public Factura Factura { get; set; } = new();
    }

    [XmlRoot("factura")]
    public class Factura {
        [XmlAttribute("id")]
        public string Id { get; set; } = "comprobante";

        [XmlAttribute("version")]
        public string Version { get; set; } = "1.1.0";

        [XmlElement("infoTributaria")]
        public InfoTributaria InfoTributaria { get; set; } = new();

        [XmlElement("infoFactura")]
        public InfoFactura InfoFactura { get; set; } = new();

        [XmlArray("detalles")]
        [XmlArrayItem("detalle")]
        public List<Detalle> Detalles { get; set; } = new();

        [XmlArray("infoAdicional")]
        [XmlArrayItem("campoAdicional")]
        public List<CampoAdicional> InfoAdicional { get; set; } = new();
    }


    public class InfoTributaria {
        [XmlElement("ambiente")]
        public string? Ambiente { get; set; }

        [XmlElement("tipoEmision")]
        public string? TipoEmision { get; set; }

        [XmlElement("razonSocial")]
        public string? RazonSocial { get; set; }

        [XmlElement("nombreComercial")]
        public string? NombreComercial { get; set; }

        [XmlElement("ruc")]
        public string? Ruc { get; set; }

        [XmlElement("claveAcceso")]
        public string? ClaveAcceso { get; set; }   // <-- CAMBIO

        [XmlElement("codDoc")]
        public string? CodDoc { get; set; }

        [XmlElement("estab")]
        public string? Estab { get; set; }

        [XmlElement("ptoEmi")]
        public string? PtoEmi { get; set; }

        [XmlElement("secuencial")]
        public string? Secuencial { get; set; }    // <-- CAMBIO

        [XmlElement("dirMatriz")]
        public string? DirMatriz { get; set; }
    }


    public class InfoFactura
    {
        [XmlElement("fechaEmision")]
        public string FechaEmision { get; set; }

        [XmlElement("dirEstablecimiento")]
        public string DirEstablecimiento { get; set; }

        [XmlElement("contribuyenteEspecial")]
        public string? ContribuyenteEspecial { get; set; }
        public bool ShouldSerializeContribuyenteEspecial() {
            return !string.IsNullOrWhiteSpace(ContribuyenteEspecial);
        }


        [XmlElement("obligadoContabilidad")]
        public string ObligadoContabilidad { get; set; }

        [XmlElement("tipoIdentificacionComprador")]
        public string TipoIdentificacionComprador { get; set; }

        [XmlElement("razonSocialComprador")]
        public string RazonSocialComprador { get; set; }

        [XmlElement("identificacionComprador")]
        public string IdentificacionComprador { get; set; }

        [XmlElement("direccionComprador")]
        public string DireccionComprador { get; set; }

        [XmlIgnore]
        public decimal TotalSinImpuestos { get; set; }

        [XmlElement("totalSinImpuestos")]
        public string TotalSinImpuestosXml
        {
            get => TotalSinImpuestos.ToString("0.00", CultureInfo.InvariantCulture);
            set => TotalSinImpuestos = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlIgnore]
        public decimal TotalDescuento { get; set; }

        [XmlElement("totalDescuento")]
        public string TotalDescuentoXml
        {
            get => TotalDescuento.ToString("0.00", CultureInfo.InvariantCulture);
            set => TotalDescuento = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlArray("totalConImpuestos")]
        [XmlArrayItem("totalImpuesto")]
        public List<TotalImpuesto> TotalConImpuestos { get; set; }

        [XmlIgnore]
        public decimal Propina { get; set; }

        [XmlElement("propina")]
        public string PropinaXml
        {
            get => Propina.ToString("0.00", CultureInfo.InvariantCulture);
            set => Propina = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlIgnore]
        public decimal ImporteTotal { get; set; }

        [XmlElement("importeTotal")]
        public string ImporteTotalXml
        {
            get => ImporteTotal.ToString("0.00", CultureInfo.InvariantCulture);
            set => ImporteTotal = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlElement("moneda")]
        public string Moneda { get; set; }

        [XmlArray("pagos")]
        [XmlArrayItem("pago")]
        public List<Pago> Pagos { get; set; }
    }

    public class TotalImpuesto {
        [XmlElement("codigo")]
        public string Codigo { get; set; }

        [XmlElement("codigoPorcentaje")]
        public string CodigoPorcentaje { get; set; }

        [XmlIgnore]
        public decimal BaseImponible { get; set; }

        [XmlElement("baseImponible")]
        public string BaseImponibleXml {
            get => BaseImponible.ToString("0.00", CultureInfo.InvariantCulture);
            set => BaseImponible = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlIgnore]
        public decimal Valor { get; set; }

        [XmlElement("valor")]
        public string ValorXml {
            get => Valor.ToString("0.00", CultureInfo.InvariantCulture);
            set => Valor = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        // Si quieres tener DescuentoAdicional para BD, déjalo pero NO lo serialices
        [XmlIgnore]
        public decimal DescuentoAdicional { get; set; }
    }


    public class Pago
    {
        [XmlElement("formaPago")]
        public string FormaPago { get; set; }

        [XmlIgnore]
        public decimal Total { get; set; }

        [XmlElement("total")]
        public string TotalXml
        {
            get => Total.ToString("0.00", CultureInfo.InvariantCulture);
            set => Total = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlElement("plazo")]
        public string Plazo { get; set; }

        [XmlElement("unidadTiempo")]
        public string UnidadTiempo { get; set; }
    }

    public class Detalle
    {
        [XmlElement("codigoPrincipal")]
        public string CodigoPrincipal { get; set; }

        [XmlElement("codigoAuxiliar")]
        public string CodigoAuxiliar { get; set; }

        [XmlElement("descripcion")]
        public string Descripcion { get; set; }

        [XmlIgnore]
        public decimal Cantidad { get; set; }

        [XmlElement("cantidad")]
        public string CantidadXml
        {
            get => Cantidad.ToString("0.00", CultureInfo.InvariantCulture);
            set => Cantidad = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlIgnore]
        public decimal PrecioUnitario { get; set; }

        [XmlElement("precioUnitario")]
        public string PrecioUnitarioXml
        {
            get => PrecioUnitario.ToString("0.00", CultureInfo.InvariantCulture);
            set => PrecioUnitario = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlIgnore]
        public decimal Descuento { get; set; }

        [XmlElement("descuento")]
        public string DescuentoXml
        {
            get => Descuento.ToString("0.00", CultureInfo.InvariantCulture);
            set => Descuento = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlIgnore]
        public decimal PrecioTotalSinImpuesto { get; set; }

        [XmlElement("precioTotalSinImpuesto")]
        public string PrecioTotalSinImpuestoXml
        {
            get => PrecioTotalSinImpuesto.ToString("0.00", CultureInfo.InvariantCulture);
            set => PrecioTotalSinImpuesto = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlArray("impuestos")]
        [XmlArrayItem("impuesto")]
        public List<ImpuestoDetalle> Impuestos { get; set; }
    }

    public class ImpuestoDetalle
    {
        [XmlElement("codigo")]
        public string Codigo { get; set; }

        [XmlElement("codigoPorcentaje")]
        public string CodigoPorcentaje { get; set; }

        [XmlIgnore]
        public decimal Tarifa { get; set; }

        [XmlElement("tarifa")]
        public string TarifaXml
        {
            get => Tarifa.ToString("0.00", CultureInfo.InvariantCulture);
            set => Tarifa = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlIgnore]
        public decimal BaseImponible { get; set; }

        [XmlElement("baseImponible")]
        public string BaseImponibleXml
        {
            get => BaseImponible.ToString("0.00", CultureInfo.InvariantCulture);
            set => BaseImponible = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        [XmlIgnore]
        public decimal Valor { get; set; }

        [XmlElement("valor")]
        public string ValorXml
        {
            get => Valor.ToString("0.00", CultureInfo.InvariantCulture);
            set => Valor = decimal.Parse(value, CultureInfo.InvariantCulture);
        }
    }

    public class CampoAdicional
    {
        [XmlAttribute("nombre")]
        public string Nombre { get; set; }

        [XmlText]
        public string Valor { get; set; }
    }

    public class InfoDelCliente {
        public string? FechaEmision { get; set; }       // opcional si lo pones tú
        public string? TipoComprobante { get; set; }    // opcional si lo pones tú
        public string? Ruc { get; set; }                // tú lo sacas de empresa

        public string? TipoAmbiente { get; set; }       // ok
        public string? Serie { get; set; }              // tú lo calculas
        public string? Secuencial { get; set; }         // <-- CAMBIO
        public string? CodigoNumerico { get; set; }     // tú lo calculas
        public string? TipoEmision { get; set; }        // tú lo fijas

        public string Token { get; set; } = default!;
        public int Id_aplicacion { get; set; }
        public int Id_empresa { get; set; }
        public int Id_persona { get; set; }
        public string ContrasenaFirma { get; set; } = default!;
    }

}
