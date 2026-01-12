using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.XMP.Impl;
using iText.Signatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Pkcs;
using QRCoder;
using SkiaSharp;

namespace ApiFacturacion.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase {

        /// <summary>
        /// Endpoint principal: firma uno o varios PDFs usando un certificado .p12
        /// </summary>
        /// <param name="pdfFiles">Archivos PDF a firmar</param>
        /// <param name="p12File">Archivo de certificado en formato .p12</param>
        /// <param name="password">Contraseña del .p12</param>

        [HttpPost("firmar")]
        public IActionResult SignPdfs(
    [FromForm] List<IFormFile> pdfFiles,
    IFormFile p12File,
    [FromForm] string password,
    [FromForm] string x,
    [FromForm] string y,
    [FromForm] int page) {
            // Validaciones iniciales
            if (pdfFiles == null || pdfFiles.Count == 0 || p12File == null || string.IsNullOrEmpty(password))
                return BadRequest("Se requieren los archivos PDF, el certificado .p12 y la contraseña.");

            // Parseo seguro de coordenadas
            double parsedX, parsedY;

            if (!double.TryParse(x, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsedX) ||
                !double.TryParse(y, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsedY))
                return BadRequest($"Coordenadas inválidas. X='{x}', Y='{y}'");

            try {
                // Leer el archivo .p12 en memoria
                using (var p12Stream = new MemoryStream()) {
                    p12File.CopyTo(p12Stream);

                    // Lista para almacenar los PDFs firmados
                    var signedPdfs = new List<(string FileName, byte[] FileContent)>();

                    foreach (var pdfFile in pdfFiles) {
                        using (var pdfStream = new MemoryStream()) {
                            pdfFile.CopyTo(pdfStream);

                            // Firmar el PDF actual
                            var signedPdfBytes = SignPdfWithP12(pdfStream, p12Stream, password, parsedX, parsedY, page);

                            // Agregar a la lista el PDF firmado
                            signedPdfs.Add((pdfFile.FileName, signedPdfBytes));
                        }
                    }

                    // Si es un solo archivo, devolverlo directamente
                    if (signedPdfs.Count == 1) {
                        var signedPdf = signedPdfs.First();
                        return File(signedPdf.FileContent, "application/pdf", signedPdf.FileName);
                    }

                    // Si son varios, devolver ZIP
                    using (var zipStream = new MemoryStream()) {
                        using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true)) {
                            foreach (var signedPdf in signedPdfs) {
                                var entry = archive.CreateEntry(signedPdf.FileName);
                                using (var entryStream = entry.Open()) {
                                    entryStream.Write(signedPdf.FileContent, 0, signedPdf.FileContent.Length);
                                }
                            }
                        }
                        return File(zipStream.ToArray(), "application/zip", "documentos_firmados.zip");
                    }
                }
            } catch (Exception ex) {
                Utils.Logger.Log("Error en SignPdfs", ex);
                return StatusCode(500, $"Error al firmar los PDFs: {ex.Message}");
            }
        }


        /// <summary>
        /// Firma digitalmente un PDF con el certificado .p12
        /// </summary>
        private byte[] SignPdfWithP12(Stream pdfStream, Stream p12Stream, string password, double x, double y, int page) { 
            try {
                // Reiniciar la posición de los flujos
                pdfStream.Position = 0;
                p12Stream.Position = 0;

                if (p12Stream.Length == 0) {
                    throw new Exception("El archivo .p12 está vacío o no se cargó correctamente.");
                }

                using (PdfReader reader = new PdfReader(pdfStream))
                using (MemoryStream destStream = new MemoryStream()) {
                    PdfSigner signer = new PdfSigner(reader, destStream, new StampingProperties());

                    // Cargar el certificado y la clave privada desde el .p12
                    Pkcs12Store store = new Pkcs12Store(p12Stream, password.ToCharArray());
                    string alias = null;

                    // Selección dinámica del alias en base a KeyUsage
                    foreach (string currentAlias in store.Aliases.Cast<string>()) {
                        if (store.IsKeyEntry(currentAlias)) {
                            var certEntry = store.GetCertificate(currentAlias);
                            var cert = certEntry.Certificate;
                            var keyUsage = cert.GetKeyUsage();

                            bool usableForSigning = false;

                            if (keyUsage != null) {
                                // Verificamos digitalSignature (0) o nonRepudiation (1)
                                if ((keyUsage.Length > 0 && keyUsage[0]) ||
                                    (keyUsage.Length > 1 && keyUsage[1])) {
                                    usableForSigning = true;
                                }
                            } else {
                                // Si no tiene KeyUsage definido, asumimos que sirve
                                usableForSigning = true;
                            }

                            if (usableForSigning) {
                                alias = currentAlias;

                                // Log de depuración
                                var usages = new List<string>();
                                if (keyUsage != null) {
                                    if (keyUsage.Length > 0 && keyUsage[0]) usages.Add("digitalSignature");
                                    if (keyUsage.Length > 1 && keyUsage[1]) usages.Add("nonRepudiation");
                                }
                                Console.WriteLine($"Alias seleccionado: {alias} - KeyUsage: {string.Join(", ", usages)}");
                                break;
                            }
                        }
                    }

                    if (alias == null) {
                        throw new Exception("No se encontró una entrada de clave en el archivo .p12.");
                    }

                    var pk = store.GetKey(alias).Key;

                    // Construir la cadena de certificados
                    var chain = store.GetCertificateChain(alias)
                                     .Select(c => c.Certificate)
                                     .ToArray();

                    if (chain == null || chain.Length == 0) {
                        throw new Exception("La cadena de certificados no es válida.");
                    }

                    // Extraer Common Name (CN) del certificado
                    string subject = chain[0].SubjectDN.ToString();
                    string commonName = ExtractCnFromSubject(subject);

                    // Crear el firmador externo
                    IExternalSignature externalSignature = new PrivateKeySignature(pk, DigestAlgorithms.SHA256);

                    // Configuración de la apariencia de la firma
                    signer.GetSignatureAppearance()
                          .SetReason("Firma digital")
                          .SetLocation("Mi ubicación")
                          .SetReuseAppearance(false);

                    // Datos para QR y texto en el documento
                    string fechaActual = DateTime.Now.ToString("dd/MM/yyyy");
                    string contenidoQr = $"{commonName} - Documento emitido: {fechaActual}";
                    string textoFirma = "Firmado electrónicamente por:";
                    string nombreCompleto = FormatCnForDisplay(ExtractCnFromSubject(subject));
                    string fechaEmision = "Fecha de emisión: 05/01/2025";

                    // Insertar QR y texto en todas las páginas
                    //for (int i = 1; i <= signer.GetDocument().GetNumberOfPages(); i++) {
                    //    AgregarQrYTexto(signer.GetDocument(), contenidoQr, textoFirma, nombreCompleto, fechaEmision, i);
                    //}

                    // El front ya entrega coordenadas PDF con origen abajo-izquierda.
                    double iTextY = y;

                    AgregarQrYTexto(
                        signer.GetDocument(),
                        contenidoQr,
                        textoFirma,
                        nombreCompleto,
                        fechaActual,
                        page,
                        x,
                        iTextY
                    );

                    // Aplicar la firma digital al PDF
                    signer.SignDetached(externalSignature, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);

                    return destStream.ToArray();
                }
            } catch (Exception ex) {
                Utils.Logger.Log("Error en SignPdfs", ex);
                throw;
            }
        }

        /// <summary>
        /// Extrae el Common Name (CN) del Subject en el certificado
        /// </summary>
        private string ExtractCnFromSubject(string subject) {
            var match = System.Text.RegularExpressions.Regex.Match(subject, @"CN=([^,]+)");
            if (match.Success) {
                return match.Groups[1].Value;
            }
            throw new Exception("No se pudo encontrar el CN en el sujeto del certificado.");
        }

        /// <summary>
        /// Formatea el CN separando nombres y apellidos
        /// </summary>
        private string FormatCnForDisplay(string cn) {
            string[] parts = cn.Split(' ');

            if (parts.Length < 2) {
                throw new Exception("El CN no tiene un formato válido para dividir en nombres y apellidos.");
            }

            // Asumimos que los dos primeros elementos son nombres y el resto apellidos
            string nombres = string.Join(" ", parts.Take(2));
            string apellidos = string.Join(" ", parts.Skip(2));

            return $"{nombres}\n{apellidos}";
        }

        /// <summary>
        /// Inserta un QR y textos (firma, nombre, fecha) en una página PDF
        /// </summary>
        private void AgregarQrYTexto(
            PdfDocument pdfDoc,
            string contenidoQr,
            string textoFirma,
            string nombreCompleto,
            string fechaEmision,
            int numeroPagina,
            double clickX,
            double clickY) {
            PdfPage page = pdfDoc.GetPage(numeroPagina);

            // Tamaños en puntos PDF
            float qrWidth = 60f;
            float qrHeight = 60f;
            float textBoxWidth = 200f;
            float gap = 10f; // separación entre QR y texto

            // Centrar el QR en el punto clicado
            float qrX = (float)(clickX - qrWidth / 2f);
            float qrY = (float)(clickY - qrHeight / 2f);

            // Desplazar ligeramente a la izquierda (opcional)
            //qrX -= 40f;

            // Coordenadas iniciales para el texto (arriba del bloque)
            float textX = qrX + qrWidth + gap;
            float textYTop = qrY + qrHeight - 16f; // línea superior base

            // Generar QR
            SKBitmap qrBitmap = GenerarCodigoQr(contenidoQr, 5);
            using (var qrStream = new MemoryStream()) {
                qrBitmap.Encode(qrStream, SKEncodedImageFormat.Png, 100);
                byte[] qrImageData = qrStream.ToArray();
                ImageData qrImage = ImageDataFactory.Create(qrImageData);

                var qrCanvasImage = new iText.Layout.Element.Image(qrImage)
                    .SetFixedPosition(numeroPagina, qrX, qrY, qrWidth);

                var fontBold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                // Ajustar saltos de línea si el nombreCompleto ya contiene '\n'
                var lineasNombre = nombreCompleto.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                string nombreLinea1 = lineasNombre.Length > 0 ? lineasNombre[0] : "";
                string nombreLinea2 = lineasNombre.Length > 1 ? lineasNombre[1] : "";

                // Definir espaciado vertical
                float lineSpacing = 12f;

                // Crear los párrafos con separación más generosa
                var p1 = new iText.Layout.Element.Paragraph(textoFirma)
                    .SetFont(fontBold).SetFontSize(8)
                    .SetFixedPosition(numeroPagina, textX, textYTop, textBoxWidth);

                var p2 = new iText.Layout.Element.Paragraph(nombreLinea1)
                    .SetFont(font).SetFontSize(9)
                    .SetFixedPosition(numeroPagina, textX, textYTop - lineSpacing, textBoxWidth);

                var p3 = new iText.Layout.Element.Paragraph(nombreLinea2)
                    .SetFont(font).SetFontSize(9)
                    .SetFixedPosition(numeroPagina, textX, textYTop - (lineSpacing * 2), textBoxWidth);

                var p4 = new iText.Layout.Element.Paragraph($"Fecha: {fechaEmision}")
                    .SetFont(font).SetFontSize(8)
                    .SetFixedPosition(numeroPagina, textX, textYTop - (lineSpacing * 3.2f), textBoxWidth);

                var canvas = new iText.Layout.Canvas(new PdfCanvas(page), pdfDoc, page.GetPageSize());
                canvas.Add(qrCanvasImage);
                canvas.Add(p1);
                canvas.Add(p2);
                if (!string.IsNullOrEmpty(nombreLinea2))
                    canvas.Add(p3);
                canvas.Add(p4);
            }
        }

        /// <summary>
        /// Genera un código QR como SKBitmap
        /// </summary>
        private static SKBitmap GenerarCodigoQr(string text, int size) {
            using var qrGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(size);

            using var ms = new MemoryStream(qrCodeAsPngByteArr);
            return SKBitmap.Decode(ms);
        }
    }
}
