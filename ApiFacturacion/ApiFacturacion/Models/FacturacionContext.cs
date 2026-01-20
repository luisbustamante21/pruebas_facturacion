using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ApiFacturacion.Models;

public partial class FacturacionContext : DbContext
{
    public FacturacionContext()
    {
    }

    public FacturacionContext(DbContextOptions<FacturacionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FactEmpresa> FactEmpresas { get; set; }

    public virtual DbSet<FactEstablecimiento> FactEstablecimientos { get; set; }

    public virtual DbSet<FactFactura> FactFacturas { get; set; }

    public virtual DbSet<FactFacturaDetalle> FactFacturaDetalles { get; set; }

    public virtual DbSet<FactFacturaInfoAdicional> FactFacturaInfoAdicionals { get; set; }

    public virtual DbSet<FactFacturaPago> FactFacturaPagos { get; set; }

    public virtual DbSet<FactFacturaPdf> FactFacturaPdfs { get; set; }

    public virtual DbSet<FactFacturaTotalImpuesto> FactFacturaTotalImpuestos { get; set; }

    public virtual DbSet<FactFacturaXml> FactFacturaXmls { get; set; }

    public virtual DbSet<FactParametro> FactParametros { get; set; }

    public virtual DbSet<FactPuntoEmision> FactPuntoEmisions { get; set; }

    public virtual DbSet<FacturaDetalleImpuesto> FacturaDetalleImpuestos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=168.231.73.39;Port=5432;Database=Facturacion;Username=postgres;Password=MedDev12345@");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FactEmpresa>(entity =>
        {
            entity.HasKey(e => e.Idfactempresa).HasName("FactEmpresa_pkey");

            entity.ToTable("FactEmpresa");

            entity.HasIndex(e => e.Ruc, "FactEmpresa_ruc_key").IsUnique();

            entity.Property(e => e.Idfactempresa).HasColumnName("idfactempresa");
            entity.Property(e => e.ContribuyenteEspecial)
                .HasMaxLength(20)
                .HasColumnName("contribuyente_especial");
            entity.Property(e => e.DirMatriz)
                .HasMaxLength(255)
                .HasColumnName("dir_matriz");
            entity.Property(e => e.FirmaDigital).HasColumnName("firma_digital");
            entity.Property(e => e.IdAplicacion).HasColumnName("id_aplicacion");
            entity.Property(e => e.IdEmpresa).HasColumnName("id_empresa");
            entity.Property(e => e.IdPersona).HasColumnName("id_persona");
            entity.Property(e => e.Logo).HasColumnName("logo");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(255)
                .HasColumnName("nombre_comercial");
            entity.Property(e => e.ObligadoContabilidad)
                .HasMaxLength(2)
                .HasColumnName("obligado_contabilidad");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(255)
                .HasColumnName("razon_social");
            entity.Property(e => e.RegimenMicroempresa)
                .HasMaxLength(2)
                .HasColumnName("regimen_microempresa");
            entity.Property(e => e.Ruc)
                .HasMaxLength(13)
                .HasColumnName("ruc");
            entity.Property(e => e.Token).HasColumnName("token");
        });

        modelBuilder.Entity<FactEstablecimiento>(entity =>
        {
            entity.HasKey(e => e.Idfactestablecimiento).HasName("FactEstablecimiento_pkey");

            entity.ToTable("FactEstablecimiento");

            entity.Property(e => e.Idfactestablecimiento).HasColumnName("idfactestablecimiento");
            entity.Property(e => e.Codigo)
                .HasMaxLength(3)
                .HasColumnName("codigo");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .HasColumnName("direccion");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");

            entity.HasOne(d => d.Empresa).WithMany(p => p.FactEstablecimientos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FactEstablecimiento_empresa_id_fkey");
        });

        modelBuilder.Entity<FactFactura>(entity =>
        {
            entity.HasKey(e => e.Idfactfactura).HasName("FactFactura_pkey");

            entity.ToTable("FactFactura");

            entity.Property(e => e.Idfactfactura).HasColumnName("idfactfactura");
            entity.Property(e => e.Ambiente).HasColumnName("ambiente");
            entity.Property(e => e.ClaveAcceso)
                .HasMaxLength(49)
                .HasColumnName("clave_acceso");
            entity.Property(e => e.CodDoc)
                .HasMaxLength(2)
                .HasColumnName("cod_doc");
            entity.Property(e => e.DireccionComprador)
                .HasMaxLength(255)
                .HasColumnName("direccion_comprador");
            entity.Property(e => e.FacturaId)
                .HasMaxLength(50)
                .HasColumnName("factura_id");
            entity.Property(e => e.FechaEmision).HasColumnName("fecha_emision");
            entity.Property(e => e.IdentificacionComprador)
                .HasMaxLength(20)
                .HasColumnName("identificacion_comprador");
            entity.Property(e => e.ImporteTotal)
                .HasPrecision(14, 2)
                .HasColumnName("importe_total");
            entity.Property(e => e.Moneda)
                .HasMaxLength(10)
                .HasColumnName("moneda");
            entity.Property(e => e.Propina)
                .HasPrecision(14, 2)
                .HasColumnName("propina");
            entity.Property(e => e.PuntoEmisionId).HasColumnName("punto_emision_id");
            entity.Property(e => e.RazonSocialComprador)
                .HasMaxLength(255)
                .HasColumnName("razon_social_comprador");
            entity.Property(e => e.Secuencial)
                .HasMaxLength(9)
                .HasColumnName("secuencial");
            entity.Property(e => e.TipoEmision)
                .HasMaxLength(2)
                .HasColumnName("tipo_emision");
            entity.Property(e => e.TipoIdentificacionComprador)
                .HasMaxLength(2)
                .HasColumnName("tipo_identificacion_comprador");
            entity.Property(e => e.TotalDescuento)
                .HasPrecision(14, 2)
                .HasColumnName("total_descuento");
            entity.Property(e => e.TotalSinImpuestos)
                .HasPrecision(14, 2)
                .HasColumnName("total_sin_impuestos");
            entity.Property(e => e.Version)
                .HasMaxLength(10)
                .HasColumnName("version");

            entity.HasOne(d => d.PuntoEmision).WithMany(p => p.FactFacturas)
                .HasForeignKey(d => d.PuntoEmisionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FactFactura_punto_emision_id_fkey");
        });

        modelBuilder.Entity<FactFacturaDetalle>(entity =>
        {
            entity.HasKey(e => e.Idfactfacturadetalle).HasName("FactFacturaDetalle_pkey");

            entity.ToTable("FactFacturaDetalle");

            entity.Property(e => e.Idfactfacturadetalle).HasColumnName("idfactfacturadetalle");
            entity.Property(e => e.Cantidad)
                .HasPrecision(14, 6)
                .HasColumnName("cantidad");
            entity.Property(e => e.CodigoAuxiliar)
                .HasMaxLength(50)
                .HasColumnName("codigo_auxiliar");
            entity.Property(e => e.CodigoPrincipal)
                .HasMaxLength(50)
                .HasColumnName("codigo_principal");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.Descuento)
                .HasPrecision(14, 2)
                .HasColumnName("descuento");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.PrecioTotalSinImpuesto)
                .HasPrecision(14, 2)
                .HasColumnName("precio_total_sin_impuesto");
            entity.Property(e => e.PrecioUnitario)
                .HasPrecision(14, 6)
                .HasColumnName("precio_unitario");

            entity.HasOne(d => d.Factura).WithMany(p => p.FactFacturaDetalles)
                .HasForeignKey(d => d.FacturaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FactFacturaDetalle_factura_id_fkey");
        });

        modelBuilder.Entity<FactFacturaInfoAdicional>(entity =>
        {
            entity.HasKey(e => e.Idfactfacturainfoadicional).HasName("FactFacturaInfoAdicional_pkey");

            entity.ToTable("FactFacturaInfoAdicional");

            entity.Property(e => e.Idfactfacturainfoadicional).HasColumnName("idfactfacturainfoadicional");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Valor)
                .HasMaxLength(255)
                .HasColumnName("valor");

            entity.HasOne(d => d.Factura).WithMany(p => p.FactFacturaInfoAdicionals)
                .HasForeignKey(d => d.FacturaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FactFacturaInfoAdicional_factura_id_fkey");
        });

        modelBuilder.Entity<FactFacturaPago>(entity =>
        {
            entity.HasKey(e => e.Idfactfacturapago).HasName("FactFacturaPago_pkey");

            entity.ToTable("FactFacturaPago");

            entity.Property(e => e.Idfactfacturapago).HasColumnName("idfactfacturapago");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.FormaPago)
                .HasMaxLength(2)
                .HasColumnName("forma_pago");
            entity.Property(e => e.Plazo).HasColumnName("plazo");
            entity.Property(e => e.Total)
                .HasPrecision(14, 2)
                .HasColumnName("total");
            entity.Property(e => e.UnidadTiempo)
                .HasMaxLength(10)
                .HasColumnName("unidad_tiempo");

            entity.HasOne(d => d.Factura).WithMany(p => p.FactFacturaPagos)
                .HasForeignKey(d => d.FacturaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FactFacturaPago_factura_id_fkey");
        });

        modelBuilder.Entity<FactFacturaPdf>(entity =>
        {
            entity.HasKey(e => e.Idfactfacturapdf).HasName("FactFacturaPdf_pkey");

            entity.ToTable("FactFacturaPdf");

            entity.HasIndex(e => e.FacturaId, "FactFacturaPdf_factura_id_key").IsUnique();

            entity.Property(e => e.Idfactfacturapdf).HasColumnName("idfactfacturapdf");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creado_en");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.Pdf).HasColumnName("pdf");

            entity.HasOne(d => d.Factura).WithOne(p => p.FactFacturaPdf)
                .HasForeignKey<FactFacturaPdf>(d => d.FacturaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FactFacturaPdf_factura_id_fkey");
        });

        modelBuilder.Entity<FactFacturaTotalImpuesto>(entity =>
        {
            entity.HasKey(e => e.Idfactfacturatotalimpuesto).HasName("FactFacturaTotalImpuesto_pkey");

            entity.ToTable("FactFacturaTotalImpuesto");

            entity.Property(e => e.Idfactfacturatotalimpuesto).HasColumnName("idfactfacturatotalimpuesto");
            entity.Property(e => e.BaseImponible)
                .HasPrecision(14, 2)
                .HasColumnName("base_imponible");
            entity.Property(e => e.Codigo)
                .HasMaxLength(2)
                .HasColumnName("codigo");
            entity.Property(e => e.CodigoPorcentaje)
                .HasMaxLength(4)
                .HasColumnName("codigo_porcentaje");
            entity.Property(e => e.DescuentoAdicional)
                .HasPrecision(14, 2)
                .HasColumnName("descuento_adicional");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.Valor)
                .HasPrecision(14, 2)
                .HasColumnName("valor");

            entity.HasOne(d => d.Factura).WithMany(p => p.FactFacturaTotalImpuestos)
                .HasForeignKey(d => d.FacturaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FactFacturaTotalImpuesto_factura_id_fkey");
        });

        modelBuilder.Entity<FactFacturaXml>(entity =>
        {
            entity.HasKey(e => e.Idfactfacturaxml).HasName("FactFacturaXml_pkey");

            entity.ToTable("FactFacturaXml");

            entity.HasIndex(e => e.FacturaId, "FactFacturaXml_factura_id_key").IsUnique();

            entity.Property(e => e.Idfactfacturaxml).HasColumnName("idfactfacturaxml")
                .ValueGeneratedOnAdd(); ;
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creado_en");
            entity.Property(e => e.EstadoAutorizacion)
                .HasMaxLength(50)
                .HasColumnName("estado_autorizacion");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.FechaAutorizacion)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fecha_autorizacion");
            entity.Property(e => e.MensajeAutorizacion).HasColumnName("mensaje_autorizacion");
            entity.Property(e => e.NumeroAutorizacion)
                .HasMaxLength(100)
                .HasColumnName("numero_autorizacion");
            entity.Property(e => e.XmlAutorizado).HasColumnName("xml_autorizado");
            entity.Property(e => e.XmlFirmado).HasColumnName("xml_firmado");

            entity.HasOne(d => d.Factura).WithOne(p => p.FactFacturaXml)
                .HasForeignKey<FactFacturaXml>(d => d.FacturaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FactFacturaXml_factura_id_fkey");
        });

        modelBuilder.Entity<FactParametro>(entity =>
        {
            entity.HasKey(e => e.Idfactparametros).HasName("FactParametros_pkey");

            entity.Property(e => e.Idfactparametros).HasColumnName("idfactparametros");
            entity.Property(e => e.Idtipo).HasColumnName("idtipo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Valor)
                .HasMaxLength(255)
                .HasColumnName("valor");
        });

        modelBuilder.Entity<FactPuntoEmision>(entity =>
        {
            entity.HasKey(e => e.Idfactpuntoemision).HasName("FactPuntoEmision_pkey");

            entity.ToTable("FactPuntoEmision");

            entity.Property(e => e.Idfactpuntoemision).HasColumnName("idfactpuntoemision");
            entity.Property(e => e.Codigo)
                .HasMaxLength(3)
                .HasColumnName("codigo");
            entity.Property(e => e.EstablecimientoId).HasColumnName("establecimiento_id");
            entity.Property(e => e.SecuencialActual)
                .HasDefaultValue(1)
                .HasColumnName("secuencial_actual");

            entity.HasOne(d => d.Establecimiento).WithMany(p => p.FactPuntoEmisions)
                .HasForeignKey(d => d.EstablecimientoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FactPuntoEmision_establecimiento_id_fkey");
        });

        modelBuilder.Entity<FacturaDetalleImpuesto>(entity =>
        {
            entity.HasKey(e => e.Idfacturadetalleimpuesto).HasName("FacturaDetalleImpuesto_pkey");

            entity.ToTable("FacturaDetalleImpuesto");

            entity.Property(e => e.Idfacturadetalleimpuesto).HasColumnName("idfacturadetalleimpuesto");
            entity.Property(e => e.BaseImponible)
                .HasPrecision(14, 2)
                .HasColumnName("base_imponible");
            entity.Property(e => e.Codigo)
                .HasMaxLength(2)
                .HasColumnName("codigo");
            entity.Property(e => e.CodigoPorcentaje)
                .HasMaxLength(4)
                .HasColumnName("codigo_porcentaje");
            entity.Property(e => e.DetalleId).HasColumnName("detalle_id");
            entity.Property(e => e.Tarifa)
                .HasPrecision(5, 2)
                .HasColumnName("tarifa");
            entity.Property(e => e.Valor)
                .HasPrecision(14, 2)
                .HasColumnName("valor");

            entity.HasOne(d => d.Detalle).WithMany(p => p.FacturaDetalleImpuestos)
                .HasForeignKey(d => d.DetalleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FacturaDetalleImpuesto_detalle_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
