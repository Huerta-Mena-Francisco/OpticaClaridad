using Microsoft.EntityFrameworkCore;
using OpticaClaridad.Models;

namespace OpticaClaridad.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para cada modelo
        public DbSet<AbonoCredito> AbonosCredito { get; set; }
        public DbSet<CancelacionVenta> CancelacionesVenta { get; set; }
        public DbSet<CategoriaProducto> CategoriasProducto { get; set; }
        public DbSet<Cita> Citas { get; set; }
        // 🔴 ELIMINADO: DbSet<Cliente> Clientes { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }
        public DbSet<Credito> Creditos { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<ExamenVisual> ExamenesVisual { get; set; }
        public DbSet<HistorialClinico> HistorialesClinicos { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoImagen> ProductosImagenes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<UsuarioSimple> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones específicas
            modelBuilder.Entity<Producto>()
                .HasMany(p => p.Imagenes)
                .WithOne(i => i.Producto)
                .HasForeignKey(i => i.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔴 ELIMINADA la configuración de herencia Cliente-Paciente

            // Configuración de Credito - AHORA con Paciente
            modelBuilder.Entity<Credito>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Relación con Paciente
                entity.HasOne(c => c.Paciente)
                    .WithMany(p => p.Creditos)
                    .HasForeignKey(c => c.PacienteId)
                    .OnDelete(DeleteBehavior.Restrict); // No eliminar créditos si se elimina paciente

                // Relación con Abonos
                entity.HasMany(c => c.Abonos)
                    .WithOne(a => a.Credito)
                    .HasForeignKey(a => a.CreditoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Venta - AHORA solo con Paciente
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(e => e.Id);

                // 🔴 CAMBIO: Solo relación con Paciente, eliminamos Cliente
                entity.HasOne(v => v.Paciente)
                    .WithMany(p => p.Ventas)
                    .HasForeignKey(v => v.PacienteId)
                    .OnDelete(DeleteBehavior.SetNull); // Si se elimina paciente, la venta queda sin paciente asignado

                // Relación con Detalles
                entity.HasMany(v => v.Detalles)
                    .WithOne(d => d.Venta)
                    .HasForeignKey(d => d.VentaId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Crédito (uno a uno)
                entity.HasOne(v => v.Credito)
                    .WithOne(c => c.Venta)
                    .HasForeignKey<Credito>(c => c.VentaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de Paciente
            modelBuilder.Entity<Paciente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("CURRENT_DATE");
                entity.Property(e => e.Sexo).HasMaxLength(10);
                entity.Property(e => e.Observaciones).HasMaxLength(500);

                // Relación con HistorialClinico
                entity.HasMany(p => p.ExamenesVisuales)
                    .WithOne(h => h.Paciente)
                    .HasForeignKey(h => h.PacienteId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Ventas
                entity.HasMany(p => p.Ventas)
                    .WithOne(v => v.Paciente)
                    .HasForeignKey(v => v.PacienteId)
                    .OnDelete(DeleteBehavior.SetNull);

                // 🔴 NUEVA: Relación con Creditos
                entity.HasMany(p => p.Creditos)
                    .WithOne(c => c.Paciente)
                    .HasForeignKey(c => c.PacienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de HistorialClinico
            modelBuilder.Entity<HistorialClinico>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(h => h.Paciente)
                    .WithMany(p => p.ExamenesVisuales)
                    .HasForeignKey(h => h.PacienteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 🔴 ELIMINADA la configuración de Cliente (completa)
        }
    }
}