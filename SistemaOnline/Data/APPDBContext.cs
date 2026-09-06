using Microsoft.EntityFrameworkCore;
using SistemaOnline.Models;

namespace SistemaOnline.Data
{
    public class APPDBContext : DbContext
    {
        public APPDBContext(DbContextOptions<APPDBContext> options) : base(options)
        {

        }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Contrato_Empleado> Contratos_Empleados { get; set; }
        public DbSet<Contrato_Proveedor> Contratos_Proveedores { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Empleado_Turno> Empleados_Turnos { get; set; }
        public DbSet<Carta> Cartas { get; set; }
        public DbSet<Producto_Categoria> Productos_Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria_Ingrediente> Categorias_Ingredientes { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<Producto_Ingrediente> Productos_Ingredientes { get; set; }
        public DbSet<Mesa_Restaurante> Mesas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Pedido_Detalle> Pedidos_Detalles { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Comprobante_Pago> Comprobantes_Pagos { get; set; }
        public DbSet<Reservacion> Reservaciones { get; set; }
        public DbSet<Proveedor_Ingrediente> Proveedores_Ingredientes { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<Promocion> Promociones { get; set; }
        public DbSet<Producto_Promocion> Productos_Promociones { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.ID_Rol);

            modelBuilder.Entity<Rol>().HasData(
                new Rol { ID_Rol = 1, Nombre_Rol = "Administrador" },
                new Rol { ID_Rol = 2, Nombre_Rol = "Cajero" },
                new Rol { ID_Rol = 3, Nombre_Rol = "Mesero" },
                new Rol { ID_Rol = 4, Nombre_Rol = "Cliente" },
                new Rol { ID_Rol = 5, Nombre_Rol = "Chef" }
                );

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { ID_Usuario = 1, Nombre_Usuario = "Tigre Capo", Email = "admin@gmail.com", Estado = true, Password = "aQwr@2", ID_Rol = 1 },
                new Usuario { ID_Usuario = 2, Nombre_Usuario = "Carlos Chef", Email = "carlos.chef@gmail.com", Estado = true, Password = "chef123", ID_Rol = 5 },
                new Usuario { ID_Usuario = 3, Nombre_Usuario = "Ana Rojas", Email = "ana.mes@rancho.com", Estado = true, Password = "mesero456", ID_Rol = 3 },
                new Usuario { ID_Usuario = 4, Nombre_Usuario = "Julio Castillo", Email = "julio.chef@rancho.com", Estado = true, Password = "chef456", ID_Rol = 5 },
                new Usuario { ID_Usuario = 5, Nombre_Usuario = "Valeria Elizabeth", Email = "valeria.caja@rancho.com", Estado = true, Password = "caja456", ID_Rol = 2 }
                );

            // Relación 1:1 Opcional entre Empleado y Usuario
            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Usuario)
                .WithOne()
                .HasForeignKey<Empleado>(e => e.ID_Usuario)
                .IsRequired(false);

            // Contrato_Empleado hacia Empleado
            modelBuilder.Entity<Contrato_Empleado>()
                .HasOne(c => c.Empleado)
                .WithMany(e => e.Contratos)
                .HasForeignKey(c => c.ID_Empleado);

            // Contrato_Proveedor hacia Proveedor
            modelBuilder.Entity<Contrato_Proveedor>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.Contratos)
                .HasForeignKey(c => c.ID_Proveedor);

            // Empleado_Turno (tabla intermedia M:M)
            modelBuilder.Entity<Empleado_Turno>()
                .HasKey(et => new { et.ID_Turno, et.ID_Empleado });

            modelBuilder.Entity<Empleado_Turno>()
                .HasOne(et => et.Empleado)
                .WithMany(e => e.Empleado_Turnos)
                .HasForeignKey(et => et.ID_Empleado);

            modelBuilder.Entity<Empleado_Turno>()
                .HasOne(et => et.Turno)
                .WithMany(t => t.Empleado_Turnos)
                .HasForeignKey(et => et.ID_Turno);

            // Producto_Categoria hacia Carta 
            modelBuilder.Entity<Producto_Categoria>()
                .HasOne(pc => pc.Carta)
                .WithMany(c => c.Producto_Categorias)
                .HasForeignKey(pc => pc.ID_Carta);

            // Producto hacia Producto_Categoria 
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Producto_Categoria)
                .WithMany(pc => pc.Productos)
                .HasForeignKey(p => p.ID_Categoria);

            // Ingrediente hacia Categoria_Ingrediente
            modelBuilder.Entity<Ingrediente>()
                .HasOne(i => i.Categoria_Ingrediente)
                .WithMany(ci => ci.Ingredientes)
                .HasForeignKey(i => i.ID_Cat_Ingrediente);

            // Producto_Ingrediente hacia Producto 
            modelBuilder.Entity<Producto_Ingrediente>()
                .HasOne(pi => pi.Producto)
                .WithMany(p => p.Producto_Ingredientes)
                .HasForeignKey(pi => pi.ID_Producto);

            // Producto_Ingrediente hacia Ingrediente
            modelBuilder.Entity<Producto_Ingrediente>()
                .HasOne(pi => pi.Ingrediente)
                .WithMany(i => i.Producto_Ingredientes)
                .HasForeignKey(pi => pi.ID_Ingrediente);

            // Pedido hacia Empleado 
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Empleado)
                .WithMany(e => e.Pedidos)
                .HasForeignKey(p => p.ID_Empleado);

            // Pedido hacia Mesa_Restaurante
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Mesa_Restaurante)
                .WithMany(m => m.Pedidos)
                .HasForeignKey(p => p.ID_Mesa);

            // Pedido hacia Cliente (opcional)
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ID_Cliente)
                .IsRequired(false);

            // Pedido_Detalle hacia Pedido 
            modelBuilder.Entity<Pedido_Detalle>()
                .HasOne(pd => pd.Pedido)
                .WithMany(p => p.Pedido_Detalles)
                .HasForeignKey(pd => pd.ID_Pedido);

            // Pedido_Detalle hacia Producto 
            modelBuilder.Entity<Pedido_Detalle>()
                .HasOne(pd => pd.Producto)
                .WithMany(p => p.Pedido_Detalles)
                .HasForeignKey(pd => pd.ID_Producto);

            // Pago hacia Pedido 
            modelBuilder.Entity<Pago>()
                .HasOne(pa => pa.Pedido)
                .WithMany(p => p.Pagos)
                .HasForeignKey(pa => pa.ID_Pedido);

            // Comprobante_Pago hacia Pedido
            modelBuilder.Entity<Comprobante_Pago>()
                .HasOne(cp => cp.Pedido)
                .WithMany(p => p.Comprobantes_Pago)
                .HasForeignKey(cp => cp.ID_Pedido);

            // Reservacion hacia Cliente 
            modelBuilder.Entity<Reservacion>()
                .HasOne(r => r.Cliente)
                .WithMany(c => c.Reservaciones)
                .HasForeignKey(r => r.ID_Cliente);

            //  Reservacion hacia Mesa_Restaurante 
            modelBuilder.Entity<Reservacion>()
                .HasOne(r => r.Mesa_Restaurante)
                .WithMany(m => m.Reservaciones)
                .HasForeignKey(r => r.ID_Mesa);

            // Proveedor_Ingrediente (tabla intermedia M:M) 
            modelBuilder.Entity<Proveedor_Ingrediente>()
                .HasKey(pi => new { pi.ID_Proveedor, pi.ID_Ingrediente });

            modelBuilder.Entity<Proveedor_Ingrediente>()
                .HasOne(pi => pi.Proveedor)
                .WithMany(p => p.Proveedor_Ingredientes)
                .HasForeignKey(pi => pi.ID_Proveedor);

            modelBuilder.Entity<Proveedor_Ingrediente>()
                .HasOne(pi => pi.Ingrediente)
                .WithMany(i => i.Proveedor_Ingredientes)
                .HasForeignKey(pi => pi.ID_Ingrediente);

            // Inventario hacia Ingrediente
            modelBuilder.Entity<Inventario>()
                .HasOne(inv => inv.Ingrediente)
                .WithMany(i => i.Inventarios)
                .HasForeignKey(inv => inv.ID_Ingrediente);

            // Producto_Promocion (tabla intermedia M:M) 
            modelBuilder.Entity<Producto_Promocion>()
                .HasKey(pp => new { pp.ID_Producto, pp.ID_Promocion });

            modelBuilder.Entity<Producto_Promocion>()
                .HasOne(pp => pp.Producto)
                .WithMany(p => p.Producto_Promociones)
                .HasForeignKey(pp => pp.ID_Producto);

            modelBuilder.Entity<Producto_Promocion>()
                .HasOne(pp => pp.Promocion)
                .WithMany(pr => pr.Producto_Promociones)
                .HasForeignKey(pp => pp.ID_Promocion);

            base.OnModelCreating(modelBuilder);
        }
    }
}