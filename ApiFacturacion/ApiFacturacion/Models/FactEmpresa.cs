using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactEmpresa
{
    public int Idfactempresa { get; set; }

    public string RazonSocial { get; set; } = null!;

    public string? NombreComercial { get; set; }

    public string Ruc { get; set; } = null!;

    public string? DirMatriz { get; set; }

    public string? ObligadoContabilidad { get; set; }

    public string? ContribuyenteEspecial { get; set; }

    public string? RegimenMicroempresa { get; set; }

    public byte[]? FirmaDigital { get; set; }

    public string? Token { get; set; }

    public decimal? IdEmpresa { get; set; }

    public decimal? IdAplicacion { get; set; }

    public int? IdPersona { get; set; }

    public string? Logo { get; set; }

    public virtual ICollection<FactEstablecimiento> FactEstablecimientos { get; set; } = new List<FactEstablecimiento>();
}
