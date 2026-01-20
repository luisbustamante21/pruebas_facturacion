using ApiFacturacion.Interface;
using ApiFacturacion.Modelos;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text;
using System.Text.Json;
using static iText.StyledXmlParser.Css.Parse.CssDeclarationValueTokenizer;

namespace ApiFacturacion.Service
{
    public class ReportesService : IReportesService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ReportesService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<Reporte>> ObtenerInfoReportes(string token)
        {
            var urlbase = _configuration["BackPy:UrlReportes"];
            var url = "reportes/obtener_info_reportes/";

            var request = new HttpRequestMessage(HttpMethod.Post, urlbase+url)
            {
                Content = JsonContent.Create(new { })
            };

            // 👇 Header igual que Angular
            request.Headers.Add("Authorization", $"Token {token}");

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            // Deserialize al objeto que tiene la propiedad "reportes"
            var wrapper = await response.Content.ReadFromJsonAsync<ReportesResponse>();
            return wrapper?.Reportes ?? new List<Reporte>();
        }

        public async Task<byte[]?> ObtenerPdfAsync(int reportId, string clave, string token)
        {
            var urlbase = _configuration["BackPy:UrlReportes"];
            var url = "api/reportserver/pdf/";

            var body = new ObtenerPdfRequest
            {
                report_id = reportId,
                p_clave_acceso = clave
            };

            var request = new HttpRequestMessage(HttpMethod.Post, urlbase+url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json")
            };

            // 👇 IGUAL QUE ANGULAR
            request.Headers.Add("Authorization", $"Token {token}");

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            return pdfBytes;

        }

    }
}