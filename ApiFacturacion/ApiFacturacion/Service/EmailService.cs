using System;
using System.Net;
using System.Net.Mail;
using ApiFacturacion.Interface;
using Microsoft.Extensions.Hosting;

namespace ApiFacturacion.Service
{

public class EmailService : IEmailService
{
    private readonly string _host = "smtp.gmail.com";
        private readonly int _port = 587;
        //private readonly int _port = 465;
        private readonly string _user = "meddev146@gmail.com"; // Tu correo
    private readonly string _pass = "eysj zlpp vjhn pjis"; // La que generamos en el paso 1
        public EmailService()
        {
                
        }
        public async Task SendEmailAsync(string to, string subject, string body, byte[] pdfBytes, byte[] xmlBytes)
    {
            
            try
            {
                // 1. Configurar el mensaje
                MailMessage mensaje = new MailMessage(_user, to,subject, body);
                mensaje.IsBodyHtml = true; // Permite usar etiquetas HTML
                                           // Forzar el uso de protocolos de seguridad modernos


         

                // 3. Crear streams en memoria
                MemoryStream pdfStream = new MemoryStream(pdfBytes);
                MemoryStream xmlStream = new MemoryStream(xmlBytes);

                // 4. Crear adjuntos
                Attachment pdfAdjunto = new Attachment(pdfStream, "documento.pdf", "application/pdf");
                Attachment xmlAdjunto = new Attachment(xmlStream, "archivo.xml", "application/xml");

                mensaje.Attachments.Add(pdfAdjunto);
                mensaje.Attachments.Add(xmlAdjunto);
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                // 2. Configurar el cliente SMTP
                SmtpClient cliente = new SmtpClient(_host, _port)
                {
                    Credentials = new NetworkCredential(_user, _pass),
                    EnableSsl = true // Requerido por la mayoría de servidores modernos
                };

                // 3. Enviar
                cliente.Send(mensaje);
                Console.WriteLine("Correo enviado exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar: {ex.Message}");
            }
        
    }
}

}
