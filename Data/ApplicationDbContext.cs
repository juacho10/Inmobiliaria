using Microsoft.EntityFrameworkCore;
using Inmobiliaria.Models;

namespace Inmobiliaria.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Propietario> Propietarios { get; set; }
        public DbSet<Inquilino> Inquilinos { get; set; }
        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ SOLUCIÓN DateTime para PostgreSQL
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }

            // Configurar Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Nombre).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Apellido).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Password).HasMaxLength(255).IsRequired();
                entity.Property(u => u.Avatar).HasMaxLength(255);
                entity.Property(u => u.Rol).HasMaxLength(20).IsRequired();
                entity.Property(u => u.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(u => u.Activo).HasDefaultValue(true);

                // ✅ RELACIONES CORREGIDAS - Configurar las relaciones de navegación
                entity.HasMany(u => u.ContratosCreados)
                    .WithOne(c => c.UsuarioCreacion)
                    .HasForeignKey(c => c.UsuarioCreacionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ContratosModificados)
                    .WithOne(c => c.UsuarioModificacion)
                    .HasForeignKey(c => c.UsuarioModificacionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ContratosTerminados)
                    .WithOne(c => c.UsuarioTerminacion)
                    .HasForeignKey(c => c.UsuarioTerminacionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.PagosCreados)
                    .WithOne(p => p.UsuarioCreacion)
                    .HasForeignKey(p => p.UsuarioCreacionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.PagosAnulados)
                    .WithOne(p => p.UsuarioAnulacion)
                    .HasForeignKey(p => p.UsuarioAnulacionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar Propietario
            modelBuilder.Entity<Propietario>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Dni).IsUnique();
                entity.HasIndex(p => p.Email).IsUnique();
                entity.Property(p => p.Dni).HasMaxLength(10).IsRequired();
                entity.Property(p => p.Nombre).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Apellido).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Telefono).HasMaxLength(20).IsRequired();
                entity.Property(p => p.Email).HasMaxLength(100).IsRequired();
                entity.Property(p => p.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.Activo).HasDefaultValue(true);

                // Relación con Inmuebles
                entity.HasMany(p => p.Inmuebles)
                    .WithOne(i => i.Propietario)
                    .HasForeignKey(i => i.PropietarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar Inquilino
            modelBuilder.Entity<Inquilino>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasIndex(i => i.Dni).IsUnique();
                entity.HasIndex(i => i.Email).IsUnique();
                entity.Property(i => i.Dni).HasMaxLength(10).IsRequired();
                entity.Property(i => i.Nombre).HasMaxLength(50).IsRequired();
                entity.Property(i => i.Apellido).HasMaxLength(50).IsRequired();
                entity.Property(i => i.Telefono).HasMaxLength(20).IsRequired();
                entity.Property(i => i.Email).HasMaxLength(100).IsRequired();
                entity.Property(i => i.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(i => i.Activo).HasDefaultValue(true);

                // Relación con Contratos
                entity.HasMany(i => i.Contratos)
                    .WithOne(c => c.Inquilino)
                    .HasForeignKey(c => c.InquilinoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar Inmueble
            modelBuilder.Entity<Inmueble>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Direccion).HasMaxLength(200).IsRequired();
                entity.Property(i => i.Uso).HasMaxLength(20).IsRequired();
                entity.Property(i => i.Tipo).HasMaxLength(30).IsRequired();
                entity.Property(i => i.Coordenadas).HasMaxLength(100);
                entity.Property(i => i.Ambientes).IsRequired();
                entity.Property(i => i.Precio).HasPrecision(18, 2).IsRequired();
                entity.Property(i => i.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(i => i.Disponible).HasDefaultValue(true);
                entity.Property(i => i.Activo).HasDefaultValue(true);

                // Relación con Propietario
                entity.HasOne(i => i.Propietario)
                    .WithMany(p => p.Inmuebles)
                    .HasForeignKey(i => i.PropietarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Contratos
                entity.HasMany(i => i.Contratos)
                    .WithOne(c => c.Inmueble)
                    .HasForeignKey(c => c.InmuebleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar Contrato
            modelBuilder.Entity<Contrato>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.FechaInicio).IsRequired();
                entity.Property(c => c.FechaFin).IsRequired();
                entity.Property(c => c.Monto).HasPrecision(18, 2).IsRequired();
                entity.Property(c => c.FechaTerminacionAnticipada);
                entity.Property(c => c.Multa).HasPrecision(18, 2);
                entity.Property(c => c.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(c => c.Vigente).HasDefaultValue(true);
                entity.Property(c => c.Activo).HasDefaultValue(true);

                // Relación con Inmueble
                entity.HasOne(c => c.Inmueble)
                    .WithMany(i => i.Contratos)
                    .HasForeignKey(c => c.InmuebleId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Inquilino
                entity.HasOne(c => c.Inquilino)
                    .WithMany(i => i.Contratos)
                    .HasForeignKey(c => c.InquilinoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ✅ RELACIONES CON USUARIO CORREGIDAS
                entity.HasOne(c => c.UsuarioCreacion)
                    .WithMany(u => u.ContratosCreados)
                    .HasForeignKey(c => c.UsuarioCreacionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(c => c.UsuarioModificacion)
                    .WithMany(u => u.ContratosModificados)
                    .HasForeignKey(c => c.UsuarioModificacionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(c => c.UsuarioTerminacion)
                    .WithMany(u => u.ContratosTerminados)
                    .HasForeignKey(c => c.UsuarioTerminacionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Relación con Pagos
                entity.HasMany(c => c.Pagos)
                    .WithOne(p => p.Contrato)
                    .HasForeignKey(p => p.ContratoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar Pago
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.NumeroPago).IsRequired();
                entity.Property(p => p.FechaPago).IsRequired();
                entity.Property(p => p.Concepto).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Importe).HasPrecision(18, 2).IsRequired();
                entity.Property(p => p.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.Anulado).HasDefaultValue(false);
                entity.Property(p => p.Activo).HasDefaultValue(true);

                // Relación con Contrato
                entity.HasOne(p => p.Contrato)
                    .WithMany(c => c.Pagos)
                    .HasForeignKey(p => p.ContratoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ✅ RELACIONES CON USUARIO CORREGIDAS
                entity.HasOne(p => p.UsuarioCreacion)
                    .WithMany(u => u.PagosCreados)
                    .HasForeignKey(p => p.UsuarioCreacionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(p => p.UsuarioAnulacion)
                    .WithMany(u => u.PagosAnulados)
                    .HasForeignKey(p => p.UsuarioAnulacionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });
        }
    }
}
