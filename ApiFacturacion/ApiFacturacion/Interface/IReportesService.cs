using ApiFacturacion.Modelos;

namespace ApiFacturacion.Interface
{
    public interface IReportesService
    {
        Task<List<Reporte>> ObtenerInfoReportes(string token);
        Task<byte[]?> ObtenerPdfAsync(int reportId, string clave, string token);
    }
}
