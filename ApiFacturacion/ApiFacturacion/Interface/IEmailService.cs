namespace ApiFacturacion.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, byte[] pdfBytes, byte[] xmlBytes);
    }
}
