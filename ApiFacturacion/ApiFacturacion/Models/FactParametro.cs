using System;
using System.Collections.Generic;

namespace ApiFacturacion.Models;

public partial class FactParametro
{
    public int Idfactparametros { get; set; }

    public int? Idtipo { get; set; }

    public string? Nombre { get; set; }

    public string? Valor { get; set; }
}
