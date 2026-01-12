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

        //public async Task<FactEmpresa> ActualizarEmpresaAsync(DTOEmpresaRegistroDto dto)
        //{
        //    // 1. Buscar la empresa existente
        //    var empresa = await context.FactEmpresas
        //        .Include(e => e.FactEstablecimientos)
        //            .ThenInclude(es => es.FactPuntoEmisions)
        //        .FirstOrDefaultAsync(e => e.Ruc == dto.Ruc);

        //    if (empresa == null)
        //        throw new InvalidOperationException($"No existe una empresa con RUC {dto.Ruc}");

        //    // 2. Actualizar datos principales
        //    empresa.RazonSocial = dto.RazonSocial;
        //    empresa.NombreComercial = dto.NombreComercial;
        //    empresa.DirMatriz = dto.DirMatriz;
        //    empresa.ObligadoContabilidad = dto.ObligadoContabilidad;
        //    empresa.ContribuyenteEspecial = dto.ContribuyenteEspecial;
        //    empresa.RegimenMicroempresa = dto.RegimenMicroempresa;
        //    empresa.FirmaDigital = dto.FirmaDigital;
        //    empresa.IdEmpresa = dto.id_empresa;
        //    empresa.IdAplicacion = dto.id_aplicacion;
        //    empresa.IdPersona = dto.id_persona;
        //    empresa.Token = dto.Token;
        //    empresa.Logo = dto.Logo;

        //    // 3. Eliminar establecimientos actuales
        //    context.FactEstablecimientos.RemoveRange(empresa.FactEstablecimientos);

        //    // 4. Agregar establecimientos nuevos
        //    empresa.FactEstablecimientos = dto.Establecimientos.Select(est => new FactEstablecimiento
        //    {
        //        Codigo = est.Codigo,
        //        Direccion = est.Direccion,
        //        FactPuntoEmisions = est.PuntosEmision.Select(p => new FactPuntoEmision
        //        {
        //            Codigo = p.Codigo,
        //            SecuencialActual = p.Secuencial > 0 ? p.Secuencial: 1
        //        }).ToList()
        //    }).ToList();

        //    // 5. Guardar cambios
        //    await context.SaveChangesAsync();

        //    return empresa;
        //}

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


        // ============================================================
        // CREAR FACTURA COMPLETA
        // ============================================================
        //public async Task<FactFactura> CrearFacturaAsync(FactFactura factura)
        //{
        //    using var transaction = await context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // 1️⃣ Crear la factura principal
        //        context.FactFacturas.Add(factura);
        //        await context.SaveChangesAsync();

        //        // 2️⃣ Guardar detalles
        //        if (factura.FactFacturaDetalles != null)
        //        {
        //            foreach (var detalle in factura.FactFacturaDetalles)
        //            {
        //                detalle.FacturaId = factura.Idfactfactura;
        //                //detalle.Idfactfacturadetalle = 0;
        //                context.FactFacturaDetalles.Add(detalle);

        //                // Guardar impuestos por detalle
        //                if (detalle.FacturaDetalleImpuestos != null)
        //                {
        //                    foreach (var imp in detalle.FacturaDetalleImpuestos)
        //                    {
        //                        imp.DetalleId = detalle.Idfactfacturadetalle;
        //                        imp.Idfacturadetalleimpuesto = 0;
        //                        context.FacturaDetalleImpuestos.Add(imp);
        //                    }
        //                }
        //            }
        //        }

        //        // 3️⃣ Guardar totales de impuestos
        //        if (factura.FactFacturaTotalImpuestos != null)
        //        {
        //            foreach (var imp in factura.FactFacturaTotalImpuestos)
        //            {
        //                imp.FacturaId = factura.Idfactfactura;
        //                imp.Idfactfacturatotalimpuesto = 0;
        //                context.FactFacturaTotalImpuestos.Add(imp);
        //            }
        //        }

        //        // 4️⃣ Guardar pagos
        //        if (factura.FactFacturaPagos != null)
        //        {
        //            foreach (var pago in factura.FactFacturaPagos)
        //            {
        //                pago.FacturaId = factura.Idfactfactura;
        //                pago.Idfactfacturapago = 0;
        //                context.FactFacturaPagos.Add(pago);
        //            }
        //        }

        //        // 5️⃣ Guardar información adicional
        //        if (factura.FactFacturaInfoAdicionals != null)
        //        {
        //            foreach (var info in factura.FactFacturaInfoAdicionals)
        //            {
        //                info.FacturaId = factura.Idfactfactura;
        //                info.Idfactfacturainfoadicional = 0;
        //                context.FactFacturaInfoAdicionals.Add(info);
        //            }
        //        }

        //        // 6️⃣ Guardar XML y PDF si existen
        //        if (factura.FactFacturaXml != null)
        //        {
        //            factura.FactFacturaXml.FacturaId = factura.Idfactfactura;
        //            context.FactFacturaXmls.Add(factura.FactFacturaXml);
        //        }

        //        if (factura.FactFacturaPdf != null)
        //        {
        //            factura.FactFacturaPdf.FacturaId = factura.Idfactfactura;
        //            context.FactFacturaPdfs.Add(factura.FactFacturaPdf);
        //        }

        //        var puntoEmision = await context.FactPuntoEmisions
        //         .FirstAsync(p => p.Idfactpuntoemision == factura.PuntoEmisionId);

        //        puntoEmision.SecuencialActual += 1;

        //        context.FactPuntoEmisions.Update(puntoEmision);

        //        await context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return factura;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}
        public async Task<FactFactura> CrearFacturaAsync(FactFactura factura)
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 🚨 IMPORTANTE: Desacoplar tracking previo
                context.ChangeTracker.Clear();

                // 1️⃣ Factura
                context.FactFacturas.Add(factura);
                await context.SaveChangesAsync(); // aquí se genera IdFactFactura

                // 2️⃣ Detalles
                //if (factura.FactFacturaDetalles != null)
                //{
                //    foreach (var detalle in factura.FactFacturaDetalles)
                //    {
                //        detalle.Idfactfacturadetalle = 0; // opcional
                //        detalle.FacturaId = factura.Idfactfactura;

                //        context.FactFacturaDetalles.Add(detalle);
                //        await context.SaveChangesAsync(); // 🔑 genera ID del detalle

                //        // 2.1️⃣ Impuestos del detalle
                //        if (detalle.FacturaDetalleImpuestos != null)
                //        {
                //            foreach (var imp in detalle.FacturaDetalleImpuestos)
                //            {
                //                imp.Idfacturadetalleimpuesto = 0;
                //                imp.DetalleId = detalle.Idfactfacturadetalle;
                //                context.FacturaDetalleImpuestos.Add(imp);
                //            }
                //        }
                //    }
                //}

                // 3️⃣ Totales de impuestos
                //if (factura.FactFacturaTotalImpuestos != null)
                //{
                //    foreach (var imp in factura.FactFacturaTotalImpuestos)
                //    {
                //        imp.Idfactfacturatotalimpuesto = 0;
                //        imp.FacturaId = factura.Idfactfactura;
                //        context.FactFacturaTotalImpuestos.Add(imp);
                //    }
                //}

                //// 4️⃣ Pagos
                //if (factura.FactFacturaPagos != null)
                //{
                //    foreach (var pago in factura.FactFacturaPagos)
                //    {
                //        pago.Idfactfacturapago = 0;
                //        pago.FacturaId = factura.Idfactfactura;
                //        context.FactFacturaPagos.Add(pago);
                //    }
                //}

                //// 5️⃣ Info adicional
                //if (factura.FactFacturaInfoAdicionals != null)
                //{
                //    foreach (var info in factura.FactFacturaInfoAdicionals)
                //    {
                //        info.Idfactfacturainfoadicional = 0;
                //        info.FacturaId = factura.Idfactfactura;
                //        context.FactFacturaInfoAdicionals.Add(info);
                //    }
                //}

                // 6️⃣ XML / PDF
                if (factura.FactFacturaXml != null)
                {
                    factura.FactFacturaXml.FacturaId = factura.Idfactfactura;
                    context.FactFacturaXmls.Add(factura.FactFacturaXml);
                }

                if (factura.FactFacturaPdf != null)
                {
                    factura.FactFacturaPdf.FacturaId = factura.Idfactfactura;
                    context.FactFacturaPdfs.Add(factura.FactFacturaPdf);
                }

                // 7️⃣ Incrementar secuencial
                var puntoEmision = await context.FactPuntoEmisions
                    .FirstAsync(p => p.Idfactpuntoemision == factura.PuntoEmisionId);

                puntoEmision.SecuencialActual += 1;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return factura;
            }
            catch
            {
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
    }
}
