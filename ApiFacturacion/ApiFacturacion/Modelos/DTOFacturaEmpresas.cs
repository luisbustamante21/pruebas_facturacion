namespace ApiFacturacion.Modelos
{   
    public class DTOEmpresaRegistroDto
    {
        public string RazonSocial { get; set; } = null!;
        public string? NombreComercial { get; set; }
        public string Ruc { get; set; } = null!;
        public string? DirMatriz { get; set; }
        public string? ObligadoContabilidad { get; set; }
        public string? ContribuyenteEspecial { get; set; }
        public string? RegimenMicroempresa { get; set; }
        public byte[]? FirmaDigital { get; set; }
        public string Token {  get; set; }  
        public string Logo {  get; set; }  
        public int id_empresa { get; set; }
        public int id_aplicacion { get; set; }
        public int id_persona { get; set; }
        public List<DTOEstablecimientoDto> Establecimientos { get; set; } = new();
    }

    public class DTOEstablecimientoDto
    {
        public string Codigo { get; set; } = null!;
        public string? Direccion { get; set; }
        public List<DTOPuntoEmisionDto> PuntosEmision { get; set; } = new();
    }

    public class DTOPuntoEmisionDto
    {
        public string Codigo { get; set; } = null!;
        public int?  Secuencial { get;  set; }
    }
}
