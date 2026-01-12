using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactEstablecimiento
{
    public int Idfactestablecimiento { get; set; }

    public int? EmpresaId { get; set; }

    public string Codigo { get; set; } = null!;

    public string? Direccion { get; set; }

    public virtual FactEmpresa? Empresa { get; set; }

    public virtual ICollection<FactPuntoEmision> FactPuntoEmisions { get; set; } = new List<FactPuntoEmision>();
}
