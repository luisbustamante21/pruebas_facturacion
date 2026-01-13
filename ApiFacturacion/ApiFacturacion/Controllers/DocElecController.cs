using System.IO;
using System.Text;
using ApiFacturacion.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Xml;
using SRI.Recepcion;
using ApiFacturacion.utils;

using FirmaXadesNet;
using FirmaXadesNet.Signature.Parameters;
using FirmaXadesNet.Crypto;
using System.Net.Mail;
using System.Net;
using ServiceReference1;
using ApiFacturacion.Interface;
using ApiFacturacion.Models;
using System.Globalization;
using System.Xml.Linq;

namespace ApiFacturacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocElecController : ControllerBase
    {
        private readonly IFacturacionService _empresaService;
        private readonly IEmailService _emailservice;

        public DocElecController(IFacturacionService facturacionService, IEmailService emailService)
        {
            this._empresaService = facturacionService;
            this._emailservice = emailService;
        }
        private static string GenerarClaveAcceso(string clave48)
        {
            if (clave48.Length != 48)
                throw new ArgumentException("La clave debe tener 48 dígitos antes de calcular el verificador");

            int factor = 2;
            int suma = 0;

            // Recorremos de derecha a izquierda
            for (int i = clave48.Length - 1; i >= 0; i--)
            {
                int digito = int.Parse(clave48[i].ToString());
                suma += digito * factor;

                factor++;
                if (factor > 7) factor = 2; // reinicia el ciclo
            }

            int modulo = suma % 11;
            int verificador = 11 - modulo;

            if (verificador == 11) verificador = 0;
            else if (verificador == 10) verificador = 1;

            return clave48 + verificador.ToString();
        }

        private static string? SerializarRespuestaRecepcion(respuestaSolicitud? respuesta)
        {
            if (respuesta == null)
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(respuestaSolicitud));
            using var sw = new StringWriter(CultureInfo.InvariantCulture);
            serializer.Serialize(sw, respuesta);
            return sw.ToString();
        }
        [HttpPost("FirmarXmlXades", Name = "FirmarXmlXades")]
        public static void FirmarXmlXades(string xmlEntrada, string xmlSalida, byte[] rutaCertificado, string claveCert)
        {
            var flags = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable;
            var cert = new X509Certificate2(rutaCertificado, claveCert, flags);

            var xadesService = new XadesService();

            var parametros = new SignatureParameters
            {
                SignatureMethod = SignatureMethod.RSAwithSHA1, 
                DigestMethod = DigestMethod.SHA1,             
                SigningDate = DateTime.UtcNow,
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                Signer = new Signer(cert)
            };

            using var input = System.IO.File.OpenRead(xmlEntrada);
            using var output = System.IO.File.Create(xmlSalida);

            var signed = xadesService.Sign(input, parametros);
            signed.Save(output);
        }

        [HttpPost("enviar", Name = "EnviarComprobante")]
        public async Task<IActionResult> EnviarComprobanteAsync2(ValidarComprobanteRequest request)
        {


            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var info = request.Info;
            var factura = request.Factura;
            #region Validaciones token y empresa registrada
            if (info == null || info.Token == null) return BadRequest("Falta informacion de la empresa");
            if (info.Token == null) return BadRequest("Falta informacion del token");
            if (!_empresaService.VerficiarTokenExistenteValido(info.Token)) return BadRequest("Token invalido");
            var datosEmpresa = _empresaService.ObtenerEmpresaPorRucAsync(info.Id_aplicacion, info.Id_empresa, info.Id_persona);
            if (datosEmpresa == null) return BadRequest("Empresa no encontrada");
            if (!(datosEmpresa.Result.Token == info.Token)) return BadRequest("Token invalido");

            #endregion

            #region factura

            var datosEmpresaConsulta = datosEmpresa.Result;
            info.Ruc = datosEmpresaConsulta.Ruc;
            info.TipoComprobante = "01"; //Factura
            info.Secuencial = datosEmpresaConsulta.FactEstablecimientos.FirstOrDefault().FactPuntoEmisions.FirstOrDefault().SecuencialActual.ToString();
            info.Secuencial = info.Secuencial.PadLeft(9, '0');
            info.Serie = datosEmpresaConsulta.FactEstablecimientos.FirstOrDefault().Codigo
                + datosEmpresaConsulta.FactEstablecimientos.FirstOrDefault().FactPuntoEmisions.FirstOrDefault().Codigo;
            info.CodigoNumerico = "12345678";
            info.TipoEmision = "1"; //siempre va 1 emision normal 
            info.TipoAmbiente = "1"; // 1 pruebas  2 produccion , esto traer del json 
            info.FechaEmision = DateTime.Now.ToString("ddMMyyyy"); //esto tambien corregir 
                                                                   // 1. Construir clave de acceso (48 dígitos + dígito verificador módulo 11)

            string clave48 = info.FechaEmision +
                             info.TipoComprobante +
                             info.Ruc +
                             info.TipoAmbiente +
                             info.Serie +
                             info.Secuencial +
                             info.CodigoNumerico +
                             info.TipoEmision;

            var claveAcceso = GenerarClaveAcceso(clave48);

            // 2. Completar campos obligatorios
            factura.Id = "comprobante"; //tambien deberia traer del json 
            factura.Version = "1.0.0";  // tambien deberia traer del json 
            factura.InfoTributaria.ClaveAcceso = claveAcceso;
            factura.InfoTributaria.Ruc = info.Ruc;
            //factura.InfoFactura.fechaEmision = info.FechaEmision;
            factura.InfoTributaria.Ambiente = info.TipoAmbiente;
            factura.InfoTributaria.TipoEmision = info.TipoEmision;
            factura.InfoTributaria.Secuencial = info.Secuencial;
            factura.InfoTributaria.Estab = datosEmpresaConsulta.FactEstablecimientos.FirstOrDefault().Codigo;
            factura.InfoTributaria.PtoEmi = datosEmpresaConsulta.FactEstablecimientos.FirstOrDefault().FactPuntoEmisions.FirstOrDefault().Codigo;
            factura.InfoFactura.FechaEmision = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(factura.InfoFactura.DireccionComprador)) {
                factura.InfoFactura.DireccionComprador = null;
            }

            factura = CalcularFactura(factura);
            // 3. Serializar a XML UTF-8 sin BOM
            string xmlString;
            var serializer = new XmlSerializer(typeof(Factura));
            using (var sw = new Utf8StringWriter())
            {
                serializer.Serialize(sw, factura);
                xmlString = sw.ToString();
            }

            // Guardar XML sin firmar
            string archivoXml = "factura.xml";
            System.IO.File.WriteAllText(archivoXml, xmlString, Encoding.UTF8);

            // 4. Firmar XML (XAdES-BES)
            string archivoFirmado = "facturaFirmada.xml";
            FirmarXmlXades(archivoXml, archivoFirmado, datosEmpresaConsulta.FirmaDigital, info.ContrasenaFirma);
            //FirmarXmlXades(archivoXml, archivoFirmado, datosEmpresaConsulta.FirmaDigital, "Kudo1996@");

            // 5. Enviar a Recepción SRI
            var recepcionClient = new RecepcionComprobantesOfflineClient(
                RecepcionComprobantesOfflineClient.EndpointConfiguration.RecepcionComprobantesOfflinePort);

            byte[] xmlBytes = System.IO.File.ReadAllBytes(archivoFirmado);

            var responseRecepcion = await recepcionClient.validarComprobanteAsync(
                xmlBytes
            );

            if (responseRecepcion?.RespuestaRecepcionComprobante == null)
            {
                return BadRequest(new {
                    estado = (string?)null,
                    errores = new[] {
                        new {
                            identificador = "RECEPCION_NULA",
                            mensaje = "RespuestaRecepcionComprobante es null.",
                            tipo = "ERROR",
                            informacionAdicional = "Verifica el servicio de recepción y la configuración del endpoint."
                        }
                    },
                    xmlSinFirmar = xmlString,
                    xmlFirmadoBase64 = Convert.ToBase64String(xmlBytes)
                });
            }

            var estadoRecepcion =
                 responseRecepcion.RespuestaRecepcionComprobante.estado?.Trim();

            if (string.IsNullOrWhiteSpace(estadoRecepcion))
            {
                return BadRequest(new {
                    estado = estadoRecepcion,
                    errores = new[] {
                        new {
                            identificador = "ESTADO_VACIO",
                            mensaje = "El estado de recepción llegó vacío o nulo.",
                            tipo = "ERROR",
                            informacionAdicional = "Revisa el payload enviado y la respuesta del SRI."
                        }
                    },
                    xmlSinFirmar = xmlString,
                    xmlFirmadoBase64 = Convert.ToBase64String(xmlBytes)
                });
            }

            if (!string.Equals(estadoRecepcion, "RECIBIDA", StringComparison.OrdinalIgnoreCase))
            {
                var mensajes = responseRecepcion
                    .RespuestaRecepcionComprobante
                    .comprobantes?
                    .FirstOrDefault()?
                    .mensajes;

                var errores = mensajes?
                    .Select(m => new {
                        m.identificador,
                        mensaje = m.mensaje1,
                        m.tipo,
                        estadoRecepcion,
                        m.informacionAdicional
                    })
                    .ToList();

                if (errores == null || errores.Count == 0)
                {
                    errores = new List<object> {
                        new {
                            identificador = "SIN_MENSAJES",
                            mensaje = "El SRI devolvió DEVUELTA sin detalle de mensajes.",
                            tipo = "ERROR",
                            informacionAdicional = "Revisa el XML firmado y la configuración del ambiente/endpoint."
                        }
                    };
                }

                return BadRequest(new {
                    estado = estadoRecepcion,
                    errores,
                    respuestaRecepcionXml = SerializarRespuestaRecepcion(responseRecepcion.RespuestaRecepcionComprobante),
                    xmlSinFirmar = xmlString,
                    xmlFirmadoBase64 = Convert.ToBase64String(xmlBytes)
                });
            }

            #endregion

            #region validar factura

            string soapRequest = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.autorizacion"">
                    <soapenv:Header/>
                    <soapenv:Body>
                    <ec:autorizacionComprobante>
                    <claveAccesoComprobante>{claveAcceso}</claveAccesoComprobante>
                </ec:autorizacionComprobante>
                </soapenv:Body>
                </soapenv:Envelope>";

            using var client = new HttpClient();
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

            var response = await client.PostAsync(
                "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline",
                content);


            string resultXml = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(resultXml))
            {
                return BadRequest(new {
                    estado = "AUTORIZACION_SIN_RESPUESTA",
                    errores = new[] {
                        new {
                            identificador = "AUTORIZACION_HTTP",
                            mensaje = "La respuesta de autorización fue vacía o no exitosa.",
                            tipo = "ERROR",
                            informacionAdicional = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"
                        }
                    },
                    xmlSinFirmar = xmlString,
                    xmlFirmadoBase64 = Convert.ToBase64String(xmlBytes)
                });
            }

            var sri = SriSoapParser.ParseAutorizacionSri(resultXml);
            if (sri == null)
                return BadRequest("No existe nodo <autorizacion> en la respuesta del SRI.");

            var estado = sri.Estado;

            if (string.Equals(estado, "AUTORIZADO", StringComparison.OrdinalIgnoreCase)) {
                // OK: aprobado
                var numeroAut = sri.NumeroAutorizacion;
                var fechaAut = sri.FechaAutorizacion;
                var ambiente = sri.Ambiente;

                // si quieres el XML del comprobante ya decodificado:
                var xmlFacturaAutorizada = sri.ComprobanteXml;

                // aquí continúas tu flujo
            } else {
                // NO autorizado / DEVUELTA / etc.
                return BadRequest(new {
                    estado = sri.Estado,
                    numeroAutorizacion = sri.NumeroAutorizacion,
                    fechaAutorizacion = sri.FechaAutorizacion,
                    ambiente = sri.Ambiente,
                    errores = sri.Mensajes
                });
            }

            #endregion

            #region Guardar Factura
            try {
                FactFactura factFactura = new FactFactura
                {
                    ClaveAcceso = factura.InfoTributaria.ClaveAcceso,
                    CodDoc = factura.InfoTributaria.CodDoc,
                    DireccionComprador = factura.InfoFactura.DireccionComprador,
                    FechaEmision = DateOnly.ParseExact(
                    factura.InfoFactura.FechaEmision,
                    "d/M/yyyy",
                    CultureInfo.InvariantCulture
                    ),
                    IdentificacionComprador = factura.InfoFactura.IdentificacionComprador,
                    Moneda = factura.InfoFactura.Moneda,
                    //Propina = factura.InfoFactura.Propina,
                    PuntoEmision = new FactPuntoEmision
                    {
                        Codigo = factura.InfoTributaria.PtoEmi,
                        SecuencialActual = Convert.ToInt32(factura.InfoTributaria.Secuencial),
                        Establecimiento = new FactEstablecimiento
                        {
                            Codigo = factura.InfoTributaria.Estab,
                            Direccion = factura.InfoTributaria.DirMatriz
                        }
                    },


                };
                var facturaEntity = MapToFactFactura(request, datosEmpresaConsulta.FactEstablecimientos.FirstOrDefault().FactPuntoEmisions.FirstOrDefault().Idfactpuntoemision);
                var nueva = await _empresaService.CrearFacturaAsync(facturaEntity);
                //await _emailservice.SendEmailAsync("roger.baldeonc@gmail.com", "estoy probando", "Hola mundo factura de:");
                var emailDestino = factura?.InfoAdicional?.FirstOrDefault(x => (x?.Nombre ?? "").Trim().Equals("Email", StringComparison.OrdinalIgnoreCase))?.Valor?.Trim();
                emailDestino = "luis.bustamante141@gmail.com";
                if (!string.IsNullOrWhiteSpace(emailDestino))
                {
                    string cliente = "Roger Haru Baldeon Criollo";
                    string asunto = $"👋 Hola {cliente}, te enviamos tu FACTURA";

                    //string mensaje = "Tu factura ya está disponible\r\nHola, BALDEON CRIOLLO ROGER HARU\r\n\r\nTe adjuntamos tu factura, autorizado por el SRI.";
                    string mensaje = ConstruirMensajeHTML(cliente);

                    await _emailservice.SendEmailAsync(emailDestino, asunto, mensaje, xmlBytes, xmlBytes);
                }

                //return CreatedAtAction(nameof(GetById), nueva);
                return Ok(nueva);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            #endregion


            return BadRequest("Error");

        }

private Factura CalcularFactura(Factura factura)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;

        const decimal IVA_TARIFA = 15m;
        const string IVA_CODIGO = "2";
        const string IVA_CODIGO_PORCENTAJE = "4";

        decimal totalSinImpuestos = 0m;
        decimal totalIva = 0m;

        foreach (var det in factura.Detalles)
        {
            // 1️⃣ Precio total sin impuesto
            det.PrecioTotalSinImpuesto = Math.Round(
                (det.Cantidad * det.PrecioUnitario) - det.Descuento,
                2,
                MidpointRounding.AwayFromZero
            );
                //det.Cantidad = 
            totalSinImpuestos += det.PrecioTotalSinImpuesto;

            // 2️⃣ Forzar impuestos (NO confiar en el front)
            det.Impuestos = new List<ImpuestoDetalle>
        {
            new ImpuestoDetalle
            {
                Codigo = IVA_CODIGO,
                CodigoPorcentaje = IVA_CODIGO_PORCENTAJE,
                Tarifa = IVA_TARIFA,
                BaseImponible = det.PrecioTotalSinImpuesto,
                Valor = Math.Round(
                    det.PrecioTotalSinImpuesto * IVA_TARIFA / 100m,
                    2,
                    MidpointRounding.AwayFromZero
                )
            }
        };

            totalIva += (det.Impuestos[0].Valor);
        }

        // 3️⃣ InfoFactura
        factura.InfoFactura.TotalSinImpuestos = totalSinImpuestos;
        factura.InfoFactura.TotalDescuento = 0;
        factura.InfoFactura.Propina = 0;

        // 4️⃣ TotalConImpuestos
        factura.InfoFactura.TotalConImpuestos = new List<TotalImpuesto>
    {
        new TotalImpuesto
        {
            Codigo = IVA_CODIGO,
            CodigoPorcentaje = IVA_CODIGO_PORCENTAJE,
            DescuentoAdicional = 0m,
            BaseImponible = totalSinImpuestos,
            Valor = totalIva
        }
    };

        // 5️⃣ Importe total
        decimal importeTotal = totalSinImpuestos + totalIva;
        factura.InfoFactura.ImporteTotal = importeTotal;

        // 6️⃣ Pagos
        foreach (var pago in factura.InfoFactura.Pagos)
        {
            pago.Total = factura.InfoFactura.ImporteTotal;
            pago.Plazo = "0";
            pago.UnidadTiempo = "dias";
        }

        return factura;
    }

    private string ConstruirMensajeHTML(string nombreCliente)
        {
             string mensaje = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                    </head>
                    <body style='font-family: Arial, Helvetica, sans-serif; background-color: #f4f6f8; padding: 20px;'>
                        <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 25px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.08);'>
                    
                            <h2 style='color: #2c3e50; margin-top: 0;'>📄 Factura disponible</h2>
                    
                            <p style='font-size: 15px; color: #333333;'>
                                Hola <strong>{nombreCliente}</strong>,
                            </p>
                    
                            <p style='font-size: 15px; color: #333333; line-height: 1.6;'>
                                Tu factura electrónica ya se encuentra disponible.
                                En este correo te adjuntamos el archivo <strong>PDF</strong> y el
                                <strong>XML</strong> correspondiente, debidamente autorizado por el
                                <strong>SRI</strong>.
                            </p>
                    
                            <div style='background-color: #f0f4ff; padding: 15px; border-left: 4px solid #4a6cf7; margin: 20px 0;'>
                                <p style='margin: 0; font-size: 14px; color: #2c3e50;'>
                                    ✔ Documento válido para fines tributarios
                                </p>
                            </div>
                    
                            <p style='font-size: 14px; color: #555555;'>
                                Si tienes alguna duda o requieres información adicional,
                                no dudes en contactarnos.
                            </p>
                    
                            <p style='font-size: 14px; color: #555555;'>
                                Saludos cordiales,<br>
                                <strong>Departamento de Facturación</strong>
                            </p>
                    
                            <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 25px 0;'>
                    
                            <p style='font-size: 12px; color: #999999; text-align: center;'>
                                Este correo fue generado automáticamente, por favor no responder.
                            </p>
                    
                        </div>
                    </body>
                    </html>";

            return mensaje;
        }

        private FactFactura MapToFactFactura(ValidarComprobanteRequest request, int puntoEmisionId)
        {
            var facturaXml = request.Factura;
            var infoTrib = facturaXml.InfoTributaria;
            var infoFac = facturaXml.InfoFactura;

            var factura = new FactFactura
            {
                PuntoEmisionId = puntoEmisionId,

                FacturaId = facturaXml.Id,
                Version = facturaXml.Version,

                // infoTributaria
                TipoEmision = infoTrib.TipoEmision,
                ClaveAcceso = infoTrib.ClaveAcceso,
                CodDoc = infoTrib.CodDoc,
                Secuencial = infoTrib.Secuencial,

                // infoFactura
                FechaEmision = ToDateOnly(infoFac.FechaEmision),
                RazonSocialComprador = infoFac.RazonSocialComprador,
                TipoIdentificacionComprador = infoFac.TipoIdentificacionComprador,
                IdentificacionComprador = infoFac.IdentificacionComprador,
                DireccionComprador = infoFac.DireccionComprador,

                TotalSinImpuestos = (infoFac.TotalSinImpuestos),
                TotalDescuento = (infoFac.TotalDescuento),
                Propina = (infoFac.Propina),
                ImporteTotal = (infoFac.ImporteTotal),
                Moneda = infoFac.Moneda
            };

            factura.FactFacturaDetalles = MapDetalles(facturaXml.Detalles);
            factura.FactFacturaTotalImpuestos = MapTotalesImpuestos(infoFac.TotalConImpuestos);
            factura.FactFacturaPagos = MapPagos(infoFac.Pagos);
            factura.FactFacturaInfoAdicionals = MapInfoAdicional(facturaXml.InfoAdicional);

            return factura;
        }

        static decimal ToDecimal(string value)
    => decimal.Parse(value, CultureInfo.InvariantCulture);

        static DateOnly ToDateOnly(string value)
            => DateOnly.ParseExact(value, "d/M/yyyy", CultureInfo.InvariantCulture);

        private List<FactFacturaInfoAdicional> MapInfoAdicional(List<CampoAdicional> info)
        {
            if (info == null) return new();

            return info.Select(i => new FactFacturaInfoAdicional
            {
                Nombre = i.Nombre,
                Valor = i.Valor
            }).ToList();
        }

        private List<FactFacturaPago> MapPagos(List<Pago> pagos)
        {
            if (pagos == null) return new();

            return pagos.Select(p => new FactFacturaPago
            {
                FormaPago = p.FormaPago,
                Total = (p.Total),
                Plazo = int.Parse(p.Plazo),
                UnidadTiempo = p.UnidadTiempo
            }).ToList();
        }

        private List<FactFacturaTotalImpuesto> MapTotalesImpuestos(List<TotalImpuesto> impuestos)
        {
            if (impuestos == null) return new();

            return impuestos.Select(i => new FactFacturaTotalImpuesto
            {
                Codigo = i.Codigo,
                CodigoPorcentaje = i.CodigoPorcentaje,
                DescuentoAdicional =0,
                BaseImponible = (i.BaseImponible),
                Valor = (i.Valor)
            }).ToList();
        }
        private List<FactFacturaDetalle> MapDetalles(List<Detalle> detalles)
        {
            if (detalles == null) return new();

            return detalles.Select(d => new FactFacturaDetalle
            {
                CodigoPrincipal = d.CodigoPrincipal,
                CodigoAuxiliar = d.CodigoAuxiliar,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                PrecioTotalSinImpuesto = d.PrecioTotalSinImpuesto,

                FacturaDetalleImpuestos = d.Impuestos?.Select(i => new FacturaDetalleImpuesto
                {
                    Codigo = i.Codigo,
                    CodigoPorcentaje = i.CodigoPorcentaje,
                    Tarifa = (i.Tarifa),
                    BaseImponible = (i.BaseImponible),
                    Valor = (i.Valor)
                }).ToList()
            }).ToList();
        }


        //1809202501070522713000110010010000002021234567816
        [HttpPost("ValidarManualmente", Name = "ValidarManualmente")]
        public async Task<IActionResult> ValidarManualmente(string claveAcceso)
        {


            string soapRequest = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.autorizacion"">
                    <soapenv:Header/>
                    <soapenv:Body>
                    <ec:autorizacionComprobante>
                    <claveAccesoComprobante>{claveAcceso}</claveAccesoComprobante>
                </ec:autorizacionComprobante>
                </soapenv:Body>
                </soapenv:Envelope>";

            using var client = new HttpClient();
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

            var response = await client.PostAsync(
                "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline",
                content);

            string resultXml = await response.Content.ReadAsStringAsync();

            // Retorna directamente el XML como texto
            return Content(resultXml, "application/xml");
        }
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarEmpresa([FromBody] DTOEmpresaRegistroDto dto)
        {
            var empresa = await _empresaService.RegistrarEmpresaAsync(dto);
            return Ok(empresa);
        }
        [HttpPost("actualizarEmpresa")]
        public async Task<IActionResult> actualizarEmpresa([FromBody] DTOEmpresaRegistroDto dto)
        {
            var empresa = await _empresaService.ActualizarEmpresaAsync(dto);
            return Ok(empresa);
        }
        [HttpPost("ObtenerEmpresa")]
        public async Task<IActionResult> ObtenerEmpresaPorRucAsync(int idaplicacion, int idempresa, int idPersona)
        {
            var empresa = await _empresaService.ObtenerEmpresaPorRucAsync(idaplicacion, idempresa, idPersona);
            if (empresa == null)
            {
                return Ok("");
            }
            return Ok(empresa);
        }


        [HttpGet("ListarFacturas")]
        public async Task<IActionResult> GetAll() => Ok(await _empresaService.ListarFacturasAsync());

        [HttpGet("FacturasById {id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var factura = await _empresaService.ObtenerFacturaPorIdAsync(id);
            return factura != null ? Ok(factura) : NotFound();
        }

        [HttpPost("RegistrarFactura")]
        public async Task<IActionResult> Create([FromBody] FactFactura factura)
        {
            var nueva = await _empresaService.CrearFacturaAsync(factura);
            return CreatedAtAction(nameof(GetById), nueva);
        }

        [HttpPut("Update{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FactFactura factura)
        {

            var ok = await _empresaService.ActualizarFacturaAsync(factura);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("Delete{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _empresaService.EliminarFacturaAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }

    public class SriAutorizacionResult {
        public string Estado { get; set; }
        public string NumeroAutorizacion { get; set; }
        public DateTimeOffset? FechaAutorizacion { get; set; }
        public string Ambiente { get; set; }
        public string ComprobanteXml { get; set; }
        public List<SriMensaje> Mensajes { get; set; } = new();
    }

    public class SriMensaje {
        public string Identificador { get; set; }
        public string Mensaje { get; set; }
        public string InformacionAdicional { get; set; }
        public string Tipo { get; set; }
    }

    static class SriSoapParser {
        public static SriAutorizacionResult ParseAutorizacionSri(string soapXml) {
            var doc = XDocument.Parse(soapXml);

            // Primer <autorizacion> sin importar namespaces/prefijos
            var autorizacion = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "autorizacion");
            if (autorizacion == null) return null;

            string Val(string localName) =>
                autorizacion.Elements().FirstOrDefault(e => e.Name.LocalName == localName)?.Value?.Trim();

            var result = new SriAutorizacionResult {
                Estado = Val("estado"),
                NumeroAutorizacion = Val("numeroAutorizacion"),
                Ambiente = Val("ambiente")
            };

            // fechaAutorizacion: 2026-01-12T11:34:52-05:00
            var fechaTxt = Val("fechaAutorizacion");
            if (DateTimeOffset.TryParse(fechaTxt, out var dto))
                result.FechaAutorizacion = dto;

            // El comprobante viene escapado (&lt;...&gt; y &#xD;), lo decodificamos
            var compEscapado = Val("comprobante");
            if (!string.IsNullOrWhiteSpace(compEscapado)) {
                var decoded = WebUtility.HtmlDecode(compEscapado);
                result.ComprobanteXml = decoded;
            }

            // Mensajes (cuando NO es autorizado o cuando viene con observaciones)
            // En algunos casos <mensajes/> viene vacío, así que esto queda en lista vacía
            var mensajes = autorizacion
                .Descendants()
                .Where(x => x.Name.LocalName == "mensaje")
                .Select(m => new SriMensaje {
                    Identificador = m.Elements().FirstOrDefault(e => e.Name.LocalName == "identificador")?.Value?.Trim(),
                    Mensaje = m.Elements().FirstOrDefault(e => e.Name.LocalName == "mensaje")?.Value?.Trim(),
                    InformacionAdicional = m.Elements().FirstOrDefault(e => e.Name.LocalName == "informacionAdicional")?.Value?.Trim(),
                    Tipo = m.Elements().FirstOrDefault(e => e.Name.LocalName == "tipo")?.Value?.Trim(),
                })
                .ToList();

            result.Mensajes = mensajes;
            return result;
        }
    }

}
public class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}
