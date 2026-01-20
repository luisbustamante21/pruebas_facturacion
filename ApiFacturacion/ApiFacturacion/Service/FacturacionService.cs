using ApiFacturacion.Interface;
using ApiFacturacion.Modelos;
using ApiFacturacion.Models;
using Microsoft.EntityFrameworkCore;
using static iText.StyledXmlParser.Css.Parse.CssDeclarationValueTokenizer;

namespace ApiFacturacion.Service
{
    public class FacturacionService : IFacturacionService
    {
        FacturacionContext context;
        public FacturacionService(FacturacionContext _context)
        {

            this.context = _context;
        }
        public async Task<FactEmpresa> RegistrarEmpresaAsync(DTOEmpresaRegistroDto dto)
        {
            // Validar si ya existe una empresa con el mismo RUC
            if (await context.FactEmpresas.AnyAsync(e => e.Ruc == dto.Ruc))
                throw new InvalidOperationException($"Ya existe una empresa con RUC {dto.Ruc}");

            var empresa = new FactEmpresa
            {
                RazonSocial = dto.RazonSocial,
                NombreComercial = dto.NombreComercial,
                Ruc = dto.Ruc,
                DirMatriz = dto.DirMatriz,
                ObligadoContabilidad = dto.ObligadoContabilidad,
                ContribuyenteEspecial = dto.ContribuyenteEspecial,
                RegimenMicroempresa = dto.RegimenMicroempresa,
                FirmaDigital = dto.FirmaDigital,
                IdEmpresa = dto.id_empresa,
                IdAplicacion = dto.id_aplicacion,
                IdPersona = dto.id_persona,
                Token = dto.Token,
                Logo = dto.Logo,
                FactEstablecimientos = dto.Establecimientos.Select(est => new FactEstablecimiento
                {
                    Codigo = est.Codigo,
                    Direccion = est.Direccion,
                    FactPuntoEmisions = est.PuntosEmision.Select(p => new FactPuntoEmision
                    {
                        Codigo = p.Codigo,
                        SecuencialActual = 1
                    }).ToList()
                }).ToList()
            };
            try
            {
                context.FactEmpresas.Add(empresa);
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {

            }


            return empresa;
        }

        public async Task<FactEmpresa> ActualizarEmpresaAsync(DTOEmpresaRegistroDto dto)
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. Cargar empresa con relaciones
                var empresa = await context.FactEmpresas
                    .Include(e => e.FactEstablecimientos)
                        .ThenInclude(es => es.FactPuntoEmisions)
                            .ThenInclude(pe => pe.FactFacturas)
                    .FirstOrDefaultAsync(e => e.Ruc == dto.Ruc);

                if (empresa == null)
                    throw new InvalidOperationException($"Empresa con RUC {dto.Ruc} no existe");

                // 2. Update empresa (datos seguros)
                empresa.RazonSocial = dto.RazonSocial;
                empresa.NombreComercial = dto.NombreComercial;
                empresa.DirMatriz = dto.DirMatriz;
                empresa.ObligadoContabilidad = dto.ObligadoContabilidad;
                empresa.ContribuyenteEspecial = dto.ContribuyenteEspecial;
                empresa.RegimenMicroempresa = dto.RegimenMicroempresa;
                empresa.Token = dto.Token;
                empresa.Logo = dto.Logo;

                // ===============================
                // 3. ESTABLECIMIENTOS
                // ===============================
                foreach (var estDto in dto.Establecimientos)
                {
                    var establecimiento = empresa.FactEstablecimientos
                        .FirstOrDefault(e => e.Codigo == estDto.Codigo);

                    // 🔹 Nuevo establecimiento
                    if (establecimiento == null)
                    {
                        empresa.FactEstablecimientos.Add(new FactEstablecimiento
                        {
                            Codigo = estDto.Codigo,
                            Direccion = estDto.Direccion,
                            FactPuntoEmisions = estDto.PuntosEmision.Select(p => new FactPuntoEmision
                            {
                                Codigo = p.Codigo,
                                SecuencialActual = 1
                            }).ToList()
                        });

                        continue;
                    }

                    // 🔹 Update establecimiento existente
                    establecimiento.Direccion = estDto.Direccion;

                    // ===============================
                    // 4. PUNTOS DE EMISIÓN
                    // ===============================
                    foreach (var puntoDto in estDto.PuntosEmision)
                    {
                        var punto = establecimiento.FactPuntoEmisions
                            .FirstOrDefault(p => p.Codigo == puntoDto.Codigo);

                        // 🆕 Nuevo punto de emisión
                        if (punto == null)
                        {
                            establecimiento.FactPuntoEmisions.Add(new FactPuntoEmision
                            {
                                Codigo = puntoDto.Codigo,
                                SecuencialActual = 1
                            });

                            continue;
                        }

                        // ✏️ Punto existente → NO tocar secuencial si ya tiene facturas
                        if (!punto.FactFacturas.Any())
                        {
                            punto.SecuencialActual = puntoDto.Secuencial > 0
                                ? puntoDto.Secuencial
                                : punto.SecuencialActual;
                        }
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return empresa;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<FactEmpresa> ObtenerEmpresaPorRucAsync(int idaplicacion, int idempresa, int idPersona)
        {
            var empresa = await context.FactEmpresas
                .Include(e => e.FactEstablecimientos)
                    .ThenInclude(est => est.FactPuntoEmisions)
                .FirstOrDefaultAsync(e => e.IdAplicacion == idaplicacion && e.IdEmpresa == idempresa && e.IdPersona == idPersona);

            if (empresa == null)
                return null;

            return empresa;
        }

        public async Task<FactFactura> CrearFacturaAsync(FactFactura factura) {
            using var transaction = await context.Database.BeginTransactionAsync();

            try {
                context.ChangeTracker.Clear();

                // 1) Insertar factura (cabecera + relaciones si las llevas)
                context.FactFacturas.Add(factura);
                await context.SaveChangesAsync();

                // 2) Insertar XML (1 a 1)
                if (factura.FactFacturaXml != null) {
                    var yaHayXml = await context.FactFacturaXmls
                        .AnyAsync(x => x.FacturaId == factura.Idfactfactura);

                    if (!yaHayXml) {
                        factura.FactFacturaXml.Idfactfacturaxml = 0;
                        factura.FactFacturaXml.FacturaId = factura.Idfactfactura;
                        context.FactFacturaXmls.Add(factura.FactFacturaXml);
                    } else {
                        var xmlDb = await context.FactFacturaXmls
                            .FirstAsync(x => x.FacturaId == factura.Idfactfactura);

                        xmlDb.XmlFirmado = factura.FactFacturaXml.XmlFirmado;
                        xmlDb.XmlAutorizado = factura.FactFacturaXml.XmlAutorizado;
                        xmlDb.NumeroAutorizacion = factura.FactFacturaXml.NumeroAutorizacion;
                        xmlDb.FechaAutorizacion = factura.FactFacturaXml.FechaAutorizacion;
                        xmlDb.EstadoAutorizacion = factura.FactFacturaXml.EstadoAutorizacion;
                        xmlDb.MensajeAutorizacion = factura.FactFacturaXml.MensajeAutorizacion;
                    }
                }


                // 3) PDF si aplica
                if (factura.FactFacturaPdf != null) {
                    factura.FactFacturaPdf.FacturaId = factura.Idfactfactura;
                    context.FactFacturaPdfs.Add(factura.FactFacturaPdf);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return factura;
            } catch {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ============================================================
        // OBTENER FACTURA COMPLETA POR ID
        // ============================================================
        public async Task<FactFactura?> ObtenerFacturaPorIdAsync(string id)
        {
            return await context.FactFacturas
                .Include(f => f.FactFacturaDetalles)
                    .ThenInclude(d => d.FacturaDetalleImpuestos)
                .Include(f => f.FactFacturaPagos)
                .Include(f => f.FactFacturaTotalImpuestos)
                .Include(f => f.FactFacturaInfoAdicionals)
                .Include(f => f.FactFacturaXml)
                .Include(f => f.FactFacturaPdf)
                .FirstOrDefaultAsync(f => f.ClaveAcceso == id);
        }

        // ============================================================
        // LISTAR TODAS LAS FACTURAS
        // ============================================================
        public async Task<List<FactFactura>> ListarFacturasAsync()
        {
            return await context.FactFacturas
                .Include(f => f.FactFacturaDetalles)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();
        }

        // ============================================================
        // ACTUALIZAR FACTURA
        // ============================================================
        public async Task<bool> ActualizarFacturaAsync(FactFactura factura)
        {
            var facturaExistente = await context.FactFacturas
                .Include(f => f.FactFacturaDetalles)
                .FirstOrDefaultAsync(f => f.Idfactfactura == factura.Idfactfactura);

            if (facturaExistente == null)
                return false;

            // Actualizar cabecera
            context.Entry(facturaExistente).CurrentValues.SetValues(factura);

            // Actualizar detalles
            foreach (var detalle in factura.FactFacturaDetalles)
            {
                var detalleExistente = facturaExistente.FactFacturaDetalles
                    .FirstOrDefault(d => d.Idfactfacturadetalle == detalle.Idfactfacturadetalle);

                if (detalleExistente != null)
                    context.Entry(detalleExistente).CurrentValues.SetValues(detalle);
                else
                {
                    detalle.FacturaId = factura.Idfactfactura;
                    context.FactFacturaDetalles.Add(detalle);
                }
            }

            await context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // ELIMINAR FACTURA COMPLETA
        // ============================================================
        public async Task<bool> EliminarFacturaAsync(int id)
        {
            var factura = await context.FactFacturas
                .Include(f => f.FactFacturaDetalles)
                .FirstOrDefaultAsync(f => f.Idfactfactura == id);

            if (factura == null)
                return false;

            context.FactFacturas.Remove(factura);
            await context.SaveChangesAsync();
            return true;
        }

        public bool VerficiarTokenExistenteValido(string token)
        {
            bool retorno = false;

            var tokenDB = context.FactEmpresas.Where(s => s.Token == token).FirstOrDefault();
            if (tokenDB != null)
            {
                retorno = true;
            }

            return retorno;
        }

        public async Task<(int puntoEmisionId, string serie, string secuencial)> ReservarSecuencialAsync(int idaplicacion, int idempresa, int idpersona) {
            using var tx = await context.Database.BeginTransactionAsync();

            var empresa = await context.FactEmpresas
                .Include(e => e.FactEstablecimientos)
                    .ThenInclude(est => est.FactPuntoEmisions)
                .FirstAsync(e => e.IdAplicacion == idaplicacion && e.IdEmpresa == idempresa && e.IdPersona == idpersona);

            var estab = empresa.FactEstablecimientos.First();
            var pe = estab.FactPuntoEmisions.First();

            // Reservar: incrementar en DB antes de emitir
            var siguienteSec = (pe.SecuencialActual ?? 0) + 1;
            pe.SecuencialActual = siguienteSec;

            await context.SaveChangesAsync();
            await tx.CommitAsync();

            var serie = estab.Codigo + pe.Codigo;
            var secuencialStr = siguienteSec.ToString().PadLeft(9, '0');

            return (pe.Idfactpuntoemision, serie, secuencialStr);
        }

        public async Task<FactFactura?> ObtenerFacturaPorClaveAccesoAsync(string claveAcceso) {
            if (string.IsNullOrWhiteSpace(claveAcceso)) return null;

            return await context.FactFacturas
                .Include(f => f.FactFacturaDetalles)
                    .ThenInclude(d => d.FacturaDetalleImpuestos)
                .Include(f => f.FactFacturaPagos)
                .Include(f => f.FactFacturaTotalImpuestos)
                .Include(f => f.FactFacturaInfoAdicionals)
                .Include(f => f.FactFacturaXml)
                .Include(f => f.FactFacturaPdf)
                .FirstOrDefaultAsync(f => f.ClaveAcceso == claveAcceso);
        }

        public async Task ActualizarXmlEstadoAsync(string claveAcceso, Action<FactFacturaXml> apply) {
            if (string.IsNullOrWhiteSpace(claveAcceso))
                throw new ArgumentException("claveAcceso es requerida", nameof(claveAcceso));

            if (apply == null)
                throw new ArgumentNullException(nameof(apply));

            var factura = await context.FactFacturas
                .Include(f => f.FactFacturaXml)
                .FirstOrDefaultAsync(f => f.ClaveAcceso == claveAcceso);

            if (factura == null)
                throw new InvalidOperationException($"No existe factura con claveAcceso={claveAcceso}");

            if (factura.FactFacturaXml == null) {
                factura.FactFacturaXml = new FactFacturaXml {
                    FacturaId = factura.Idfactfactura,
                    CreadoEn = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };

                context.FactFacturaXmls.Add(factura.FactFacturaXml);
            }

            apply(factura.FactFacturaXml);

            await context.SaveChangesAsync();
        }


    }
}
