namespace ApiFacturacion.Modelos
{
  
    public class Reporte
    {
        public int? id { get; set; }
        public int? reportserver_report_id { get; set; }
        public string? nombre_reporte { get; set; }

    }
    public class ReportesResponse
    {
        public List<Reporte> Reportes { get; set; } = new();
    }
    public class ObtenerPdfRequest
    {
        public int report_id { get; set; }
        public string p_clave_acceso { get; set; } = string.Empty;
    }
}
