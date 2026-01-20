using ApiFacturacion.Modelos;
using ApiFacturacion.Models;

namespace ApiFacturacion.Interface
{
    public interface IFacturacionService
    {
        Task<FactEmpresa> RegistrarEmpresaAsync(DTOEmpresaRegistroDto dto);
        Task<FactEmpresa> ActualizarEmpresaAsync(DTOEmpresaRegistroDto dto);
        Task<FactEmpresa> ObtenerEmpresaPorRucAsync(int idaplicacion, int idempresa, int idPersona);
        Task<FactFactura> CrearFacturaAsync(FactFactura factura);
        Task<FactFactura?> ObtenerFacturaPorIdAsync(string id);
        Task<List<FactFactura>> ListarFacturasAsync();
        Task<bool> ActualizarFacturaAsync(FactFactura factura);
        Task<bool> EliminarFacturaAsync(int id);
        bool VerficiarTokenExistenteValido(string token);

        Task<(int puntoEmisionId, string serie, string secuencial)> ReservarSecuencialAsync(int idaplicacion, int idempresa, int idpersona);
        Task<FactFactura?> ObtenerFacturaPorClaveAccesoAsync(string claveAcceso);
        Task ActualizarXmlEstadoAsync(string claveAcceso, Action<FactFacturaXml> apply);


    }
}
