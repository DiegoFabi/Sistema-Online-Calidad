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
        public DbSet<Contrato> Contratos { get; set; }
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

            // Contrato hacia Empleado
            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.Empleado)
                .WithMany(e => e.Contratos)
                .HasForeignKey(c => c.ID_Empleado)
                .IsRequired(false);

            // Contrato hacia Proveedor
            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.Contratos)
                .HasForeignKey(c => c.ID_Proveedor)
                .IsRequired(false);

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

            modelBuilder.Entity<Categoria_Ingrediente>().HasData(
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 1, Nombre_Categoria = "Carnes" },
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 2, Nombre_Categoria = "Pescados y Mariscos" },
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 3, Nombre_Categoria = "Verduras y Hortalizas" },
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 4, Nombre_Categoria = "Frutas" },
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 5, Nombre_Categoria = "Lácteos" },
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 6, Nombre_Categoria = "Cereales y Granos" },
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 7, Nombre_Categoria = "Legumbres" },
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 8, Nombre_Categoria = "Condimentos y Especias" },
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 9, Nombre_Categoria = "Aceites y Grasas" },
                new Categoria_Ingrediente { ID_Cat_Ingrediente = 10, Nombre_Categoria = "Harinas y Derivados" }
                );

            modelBuilder.Entity<Turno>().HasData(
                new Turno { ID_Turno = 1, Nombre_Turno = "Mañana", Hora_Inicio = new TimeSpan(6, 0, 0), Hora_Fin = new TimeSpan(14, 0, 0), Dias_Semana = "Lunes-Viernes" },
                new Turno { ID_Turno = 2, Nombre_Turno = "Tarde", Hora_Inicio = new TimeSpan(14, 0, 0), Hora_Fin = new TimeSpan(22, 0, 0), Dias_Semana = "Lunes-Viernes" },
                new Turno { ID_Turno = 3, Nombre_Turno = "Noche", Hora_Inicio = new TimeSpan(22, 0, 0), Hora_Fin = new TimeSpan(6, 0, 0), Dias_Semana = "Lunes-Sábado" }
                );

            modelBuilder.Entity<Mesa_Restaurante>().HasData(
                new Mesa_Restaurante { ID_Mesa = 1, Numero_Mesa = 1, Capacidad = 4, Ubicacion = "Salón Principal", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 2, Numero_Mesa = 2, Capacidad = 4, Ubicacion = "Salón Principal", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 3, Numero_Mesa = 3, Capacidad = 4, Ubicacion = "Salón Principal", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 4, Numero_Mesa = 4, Capacidad = 6, Ubicacion = "Salón Principal", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 5, Numero_Mesa = 5, Capacidad = 6, Ubicacion = "Salón Principal", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 6, Numero_Mesa = 6, Capacidad = 6, Ubicacion = "Salón Principal", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 7, Numero_Mesa = 7, Capacidad = 2, Ubicacion = "Terraza", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 8, Numero_Mesa = 8, Capacidad = 2, Ubicacion = "Terraza", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 9, Numero_Mesa = 9, Capacidad = 4, Ubicacion = "Terraza", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 10, Numero_Mesa = 10, Capacidad = 4, Ubicacion = "Terraza", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 11, Numero_Mesa = 11, Capacidad = 8, Ubicacion = "Sala Privada", Estado = "Libre" },
                new Mesa_Restaurante { ID_Mesa = 12, Numero_Mesa = 12, Capacidad = 8, Ubicacion = "Sala Privada", Estado = "Libre" }
                );

            modelBuilder.Entity<Carta>().HasData(
                new Carta { ID_Carta = 1, Nombre_Carta = "Carta Principal", Cantidad_Platos = 30, Descripcion = "Carta principal del restaurante Rancho Sagrado", Precio = 0m },
                new Carta { ID_Carta = 2, Nombre_Carta = "Carta de Temporada", Cantidad_Platos = 10, Descripcion = "Platos especiales de temporada y eventos", Precio = 0m }
                );

            modelBuilder.Entity<Producto_Categoria>().HasData(
                new Producto_Categoria { ID_Categoria = 1, Nombre_Categoria = "Platos Principales", Descripcion = "Platos de fondo del restaurante", ID_Carta = 1 },
                new Producto_Categoria { ID_Categoria = 2, Nombre_Categoria = "Bebidas", Descripcion = "Bebidas frías y calientes", ID_Carta = 1 },
                new Producto_Categoria { ID_Categoria = 3, Nombre_Categoria = "Postres", Descripcion = "Postres y dulces para finalizar la comida", ID_Carta = 1 },
                new Producto_Categoria { ID_Categoria = 4, Nombre_Categoria = "Entradas", Descripcion = "Aperitivos y entradas del restaurante", ID_Carta = 1 },
                new Producto_Categoria { ID_Categoria = 5, Nombre_Categoria = "Especiales de Temporada", Descripcion = "Platos exclusivos de temporada", ID_Carta = 2 }
                );

            modelBuilder.Entity<Producto>().HasData(
                // Platos Principales - Cocina Peruana
                new Producto { ID_Producto = 1, Nombre_Plato = "Lomo Saltado", Descripcion = "Tiras de res salteadas con cebolla, tomate y papas fritas al estilo criollo", Tiempo_Preparacion = 20, Precio = 28.50m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 2, Nombre_Plato = "Ají de Gallina", Descripcion = "Pollo deshilachado en cremosa salsa de ají amarillo con papa y huevo duro", Tiempo_Preparacion = 35, Precio = 27.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 3, Nombre_Plato = "Ceviche Clásico", Descripcion = "Pescado fresco marinado en limón con cebolla morada, ají limo y cancha serrana", Tiempo_Preparacion = 15, Precio = 32.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 4, Nombre_Plato = "Arroz con Mariscos", Descripcion = "Arroz al estilo criollo con mezcla de mariscos frescos en salsa de ají amarillo", Tiempo_Preparacion = 30, Precio = 34.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 5, Nombre_Plato = "Seco de Res", Descripcion = "Guiso de res con cilantro, chicha de jora y frejoles verdes", Tiempo_Preparacion = 50, Precio = 26.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 6, Nombre_Plato = "Causa Limeña", Descripcion = "Masa de papa amarilla con ají amarillo rellena de atún o pollo con mayonesa", Tiempo_Preparacion = 25, Precio = 22.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 7, Nombre_Plato = "Arroz Chaufa", Descripcion = "Arroz salteado estilo chifa con huevo, cebolla china, pollo y sillao", Tiempo_Preparacion = 20, Precio = 24.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 8, Nombre_Plato = "Tacu Tacu con Lomo", Descripcion = "Tacu tacu de frejol con tiras de lomo fino y salsa criolla", Tiempo_Preparacion = 30, Precio = 31.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 9, Nombre_Plato = "Pollo a la Brasa", Descripcion = "Pollo marinado en especias criollas horneado lento con papas y ensalada", Tiempo_Preparacion = 45, Precio = 29.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 10, Nombre_Plato = "Sudado de Pescado", Descripcion = "Pescado cocido en caldo de tomate, ají y cilantro con chicha de jora", Tiempo_Preparacion = 25, Precio = 30.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 11, Nombre_Plato = "Anticuchos de Corazón", Descripcion = "Corazón de res marinado en ají panca y especias a la parrilla con choclo y papa", Tiempo_Preparacion = 20, Precio = 23.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                new Producto { ID_Producto = 12, Nombre_Plato = "Pachamanca de Cerdo", Descripcion = "Cerdo marinado en hierbas y ají, cocido en horno de tierra con habas y camote", Tiempo_Preparacion = 60, Precio = 36.00m, Disponibilidad = true, Categoria = "Platos Principales", ID_Categoria = 1 },
                // Entradas
                new Producto { ID_Producto = 13, Nombre_Plato = "Tequeños con Huancaína", Descripcion = "Tequeños de queso acompañados de salsa huancaína", Tiempo_Preparacion = 12, Precio = 14.00m, Disponibilidad = true, Categoria = "Entradas", ID_Categoria = 4 },
                new Producto { ID_Producto = 14, Nombre_Plato = "Ceviche de Champiñones", Descripcion = "Champiñones marinados en limón con ají limo y choclo desgranado", Tiempo_Preparacion = 10, Precio = 18.00m, Disponibilidad = true, Categoria = "Entradas", ID_Categoria = 4 },
                new Producto { ID_Producto = 15, Nombre_Plato = "Papa Huancaína", Descripcion = "Rodajas de papa sancochada en salsa cremosa de ají amarillo y queso fresco", Tiempo_Preparacion = 10, Precio = 16.00m, Disponibilidad = true, Categoria = "Entradas", ID_Categoria = 4 },
                // Postres
                new Producto { ID_Producto = 16, Nombre_Plato = "Picarones", Descripcion = "Buñuelos de camote y zapallo bañados en miel de chancaca con canela", Tiempo_Preparacion = 20, Precio = 14.00m, Disponibilidad = true, Categoria = "Postres", ID_Categoria = 3 },
                new Producto { ID_Producto = 17, Nombre_Plato = "Mazamorra Morada", Descripcion = "Postre de maíz morado con frutas secas y canela al estilo limeño", Tiempo_Preparacion = 30, Precio = 12.00m, Disponibilidad = true, Categoria = "Postres", ID_Categoria = 3 },
                new Producto { ID_Producto = 18, Nombre_Plato = "Arroz con Leche", Descripcion = "Postre cremoso de arroz con leche evaporada, canela y clavo de olor", Tiempo_Preparacion = 25, Precio = 11.00m, Disponibilidad = true, Categoria = "Postres", ID_Categoria = 3 },
                new Producto { ID_Producto = 19, Nombre_Plato = "Suspiro Limeño", Descripcion = "Manjar blanco cubierto de merengue italiano perfumado con Oporto", Tiempo_Preparacion = 20, Precio = 13.00m, Disponibilidad = true, Categoria = "Postres", ID_Categoria = 3 },
                // Bebidas
                new Producto { ID_Producto = 20, Nombre_Plato = "Chicha Morada", Descripcion = "Bebida peruana de maíz morado con piña, membrillo y canela", Tiempo_Preparacion = 5, Precio = 9.00m, Disponibilidad = true, Categoria = "Bebidas", ID_Categoria = 2 },
                new Producto { ID_Producto = 21, Nombre_Plato = "Pisco Sour", Descripcion = "Cóctel peruano de pisco quebranta con limón, jarabe y clara de huevo", Tiempo_Preparacion = 5, Precio = 16.00m, Disponibilidad = true, Categoria = "Bebidas", ID_Categoria = 2 },
                new Producto { ID_Producto = 22, Nombre_Plato = "Limonada Serrana", Descripcion = "Limonada natural con menta y azúcar de caña servida fría", Tiempo_Preparacion = 5, Precio = 8.00m, Disponibilidad = true, Categoria = "Bebidas", ID_Categoria = 2 },
                new Producto { ID_Producto = 23, Nombre_Plato = "Emoliente Caliente", Descripcion = "Infusión tradicional de hierbas con cebada, linaza y limón", Tiempo_Preparacion = 5, Precio = 7.00m, Disponibilidad = true, Categoria = "Bebidas", ID_Categoria = 2 },
                new Producto { ID_Producto = 24, Nombre_Plato = "Jugo de Maracuyá", Descripcion = "Jugo natural de maracuyá con azúcar y agua purificada", Tiempo_Preparacion = 5, Precio = 8.00m, Disponibilidad = true, Categoria = "Bebidas", ID_Categoria = 2 },
                new Producto { ID_Producto = 25, Nombre_Plato = "Agua Mineral", Descripcion = "Agua mineral natural o con gas en botella personal", Tiempo_Preparacion = 1, Precio = 5.00m, Disponibilidad = true, Categoria = "Bebidas", ID_Categoria = 2 },
                new Producto { ID_Producto = 26, Nombre_Plato = "Inca Kola", Descripcion = "Gaseosa nacional peruana sabor original en botella 500ml", Tiempo_Preparacion = 1, Precio = 6.00m, Disponibilidad = true, Categoria = "Bebidas", ID_Categoria = 2 },
                new Producto { ID_Producto = 27, Nombre_Plato = "Té de Hierbas", Descripcion = "Infusión de manzanilla, muña o menta con miel de abeja", Tiempo_Preparacion = 5, Precio = 6.00m, Disponibilidad = true, Categoria = "Bebidas", ID_Categoria = 2 },
                new Producto { ID_Producto = 28, Nombre_Plato = "Café Pasado", Descripcion = "Café peruano de altura preparado al paso con leche o solo", Tiempo_Preparacion = 5, Precio = 8.00m, Disponibilidad = true, Categoria = "Bebidas", ID_Categoria = 2 },
                // Especiales de Temporada
                new Producto { ID_Producto = 29, Nombre_Plato = "Cuy al Horno", Descripcion = "Cuy entero marinado en ají panca y hierbas andinas horneado con papas nativas", Tiempo_Preparacion = 60, Precio = 45.00m, Disponibilidad = true, Categoria = "Especiales de Temporada", ID_Categoria = 5 },
                new Producto { ID_Producto = 30, Nombre_Plato = "Cabrito con Frejoles", Descripcion = "Cabrito norteño estofado en chicha de jora con frejoles y yuca", Tiempo_Preparacion = 55, Precio = 38.00m, Disponibilidad = true, Categoria = "Especiales de Temporada", ID_Categoria = 5 }
                );

            modelBuilder.Entity<Empleado>().HasData(
                new Empleado { ID_Empleado = 1, Nombre = "Rosa", Apellidos = "Flores Huanca", Direccion = "Jr. Bolívar 145, Lima", Cargo = "Chef", Telefono = "912345678", Estado = "Activo", DNI = "60128374", ID_Usuario = 2 },
                new Empleado { ID_Empleado = 2, Nombre = "Ana", Apellidos = "Rojas Paredes", Direccion = "Av. Grau 901, Trujillo", Cargo = "Mesero", Telefono = "934561278", Estado = "Activo", DNI = "52109384", ID_Usuario = 3 },
                new Empleado { ID_Empleado = 3, Nombre = "Julio", Apellidos = "Castillo Medina", Direccion = "Jr. Tacna 456, Piura", Cargo = "Chef", Telefono = "946723891", Estado = "Activo", DNI = "38201745", ID_Usuario = 4 },
                new Empleado { ID_Empleado = 4, Nombre = "Valeria", Apellidos = "Elizabeth", Direccion = "Av. La Marina 233, Lima", Cargo = "Cajero", Telefono = "923456789", Estado = "Activo", DNI = "61047382", ID_Usuario = 5 }
                );

            modelBuilder.Entity<Ingrediente>().HasData(
                // Carnes (cat 1)
                new Ingrediente { ID_Ingrediente = 1, Nombre_Ingrediente = "Lomo de Res", Unidad_Medida = "kg", Descripcion = "Corte fino de res para salteados y parrilla", Costo_Unitario = 45.00m, Estado = true, ID_Cat_Ingrediente = 1 },
                new Ingrediente { ID_Ingrediente = 2, Nombre_Ingrediente = "Corazón de Res", Unidad_Medida = "kg", Descripcion = "Corazón de res para anticuchos", Costo_Unitario = 18.00m, Estado = true, ID_Cat_Ingrediente = 1 },
                new Ingrediente { ID_Ingrediente = 3, Nombre_Ingrediente = "Pollo Entero", Unidad_Medida = "kg", Descripcion = "Pollo fresco para hornear o guisar", Costo_Unitario = 12.00m, Estado = true, ID_Cat_Ingrediente = 1 },
                new Ingrediente { ID_Ingrediente = 4, Nombre_Ingrediente = "Cerdo Pierna", Unidad_Medida = "kg", Descripcion = "Pierna de cerdo para pachamanca y estofados", Costo_Unitario = 22.00m, Estado = true, ID_Cat_Ingrediente = 1 },
                new Ingrediente { ID_Ingrediente = 5, Nombre_Ingrediente = "Cuy", Unidad_Medida = "unidad", Descripcion = "Cuy andino entero para preparaciones típicas", Costo_Unitario = 35.00m, Estado = true, ID_Cat_Ingrediente = 1 },
                new Ingrediente { ID_Ingrediente = 6, Nombre_Ingrediente = "Cabrito", Unidad_Medida = "kg", Descripcion = "Cabrito norteño para guisos con chicha", Costo_Unitario = 28.00m, Estado = true, ID_Cat_Ingrediente = 1 },
                // Pescados y Mariscos (cat 2)
                new Ingrediente { ID_Ingrediente = 7, Nombre_Ingrediente = "Corvina Fresca", Unidad_Medida = "kg", Descripcion = "Corvina del litoral peruano para ceviche y sudado", Costo_Unitario = 32.00m, Estado = true, ID_Cat_Ingrediente = 2 },
                new Ingrediente { ID_Ingrediente = 8, Nombre_Ingrediente = "Pulpo", Unidad_Medida = "kg", Descripcion = "Pulpo fresco para tiradito y mariscos", Costo_Unitario = 40.00m, Estado = true, ID_Cat_Ingrediente = 2 },
                new Ingrediente { ID_Ingrediente = 9, Nombre_Ingrediente = "Langostinos", Unidad_Medida = "kg", Descripcion = "Langostinos pelados para arroz y cazuelas", Costo_Unitario = 50.00m, Estado = true, ID_Cat_Ingrediente = 2 },
                new Ingrediente { ID_Ingrediente = 10, Nombre_Ingrediente = "Conchas de Abanico", Unidad_Medida = "kg", Descripcion = "Conchas frescas de Paracas para mariscos mixtos", Costo_Unitario = 38.00m, Estado = true, ID_Cat_Ingrediente = 2 },
                // Verduras y Hortalizas (cat 3)
                new Ingrediente { ID_Ingrediente = 11, Nombre_Ingrediente = "Papa Amarilla", Unidad_Medida = "kg", Descripcion = "Papa amarilla peruana para causa y huancaína", Costo_Unitario = 4.00m, Estado = true, ID_Cat_Ingrediente = 3 },
                new Ingrediente { ID_Ingrediente = 12, Nombre_Ingrediente = "Cebolla Morada", Unidad_Medida = "kg", Descripcion = "Cebolla morada para sarsa criolla y ceviche", Costo_Unitario = 3.50m, Estado = true, ID_Cat_Ingrediente = 3 },
                new Ingrediente { ID_Ingrediente = 13, Nombre_Ingrediente = "Tomate", Descripcion = "Tomate redondo fresco para ensaladas y salsas", Unidad_Medida = "kg", Costo_Unitario = 3.00m, Estado = true, ID_Cat_Ingrediente = 3 },
                new Ingrediente { ID_Ingrediente = 14, Nombre_Ingrediente = "Choclo Peruano", Unidad_Medida = "unidad", Descripcion = "Choclo andino de grano grande para acompañamientos", Costo_Unitario = 2.00m, Estado = true, ID_Cat_Ingrediente = 3 },
                new Ingrediente { ID_Ingrediente = 15, Nombre_Ingrediente = "Cilantro Fresco", Unidad_Medida = "atado", Descripcion = "Cilantro para seco y salsas criollas", Costo_Unitario = 1.00m, Estado = true, ID_Cat_Ingrediente = 3 },
                // Frutas (cat 4)
                new Ingrediente { ID_Ingrediente = 16, Nombre_Ingrediente = "Limón Peruano", Unidad_Medida = "kg", Descripcion = "Limón peruano ácido para ceviche y pisco sour", Costo_Unitario = 3.50m, Estado = true, ID_Cat_Ingrediente = 4 },
                new Ingrediente { ID_Ingrediente = 17, Nombre_Ingrediente = "Maracuyá", Unidad_Medida = "kg", Descripcion = "Maracuyá para jugos y salsas agridulces", Costo_Unitario = 6.00m, Estado = true, ID_Cat_Ingrediente = 4 },
                new Ingrediente { ID_Ingrediente = 18, Nombre_Ingrediente = "Maíz Morado", Unidad_Medida = "kg", Descripcion = "Maíz morado seco para chicha y mazamorra", Costo_Unitario = 5.00m, Estado = true, ID_Cat_Ingrediente = 4 },
                // Lácteos (cat 5)
                new Ingrediente { ID_Ingrediente = 19, Nombre_Ingrediente = "Leche Evaporada", Unidad_Medida = "lata", Descripcion = "Leche evaporada para cremas y postres peruanos", Costo_Unitario = 4.50m, Estado = true, ID_Cat_Ingrediente = 5 },
                new Ingrediente { ID_Ingrediente = 20, Nombre_Ingrediente = "Queso Fresco", Unidad_Medida = "kg", Descripcion = "Queso fresco para salsa huancaína y causa", Costo_Unitario = 12.00m, Estado = true, ID_Cat_Ingrediente = 5 },
                // Cereales y Granos (cat 6)
                new Ingrediente { ID_Ingrediente = 21, Nombre_Ingrediente = "Arroz Extra", Unidad_Medida = "kg", Descripcion = "Arroz blanco de grano largo para acompañamientos", Costo_Unitario = 4.00m, Estado = true, ID_Cat_Ingrediente = 6 },
                new Ingrediente { ID_Ingrediente = 22, Nombre_Ingrediente = "Cebada Perlada", Unidad_Medida = "kg", Descripcion = "Cebada para emoliente y sopas", Costo_Unitario = 3.00m, Estado = true, ID_Cat_Ingrediente = 6 },
                new Ingrediente { ID_Ingrediente = 23, Nombre_Ingrediente = "Cancha Serrana", Unidad_Medida = "kg", Descripcion = "Maíz tostado para acompañar ceviche", Costo_Unitario = 4.50m, Estado = true, ID_Cat_Ingrediente = 6 },
                // Legumbres (cat 7)
                new Ingrediente { ID_Ingrediente = 24, Nombre_Ingrediente = "Frejol Canario", Unidad_Medida = "kg", Descripcion = "Frejol peruano para tacu tacu y guisos", Costo_Unitario = 6.00m, Estado = true, ID_Cat_Ingrediente = 7 },
                new Ingrediente { ID_Ingrediente = 25, Nombre_Ingrediente = "Habas Secas", Unidad_Medida = "kg", Descripcion = "Habas para pachamanca y sopas andinas", Costo_Unitario = 4.50m, Estado = true, ID_Cat_Ingrediente = 7 },
                // Condimentos y Especias (cat 8)
                new Ingrediente { ID_Ingrediente = 26, Nombre_Ingrediente = "Ají Amarillo", Unidad_Medida = "kg", Descripcion = "Ají amarillo fresco o en pasta para salsas criollas", Costo_Unitario = 8.00m, Estado = true, ID_Cat_Ingrediente = 8 },
                new Ingrediente { ID_Ingrediente = 27, Nombre_Ingrediente = "Ají Panca", Unidad_Medida = "kg", Descripcion = "Ají panca seco o en pasta para marinados", Costo_Unitario = 7.00m, Estado = true, ID_Cat_Ingrediente = 8 },
                new Ingrediente { ID_Ingrediente = 28, Nombre_Ingrediente = "Ají Limo", Unidad_Medida = "kg", Descripcion = "Ají limo picante para ceviche y tiradito", Costo_Unitario = 9.00m, Estado = true, ID_Cat_Ingrediente = 8 },
                new Ingrediente { ID_Ingrediente = 29, Nombre_Ingrediente = "Sillao", Unidad_Medida = "litro", Descripcion = "Salsa de soya para arroz chaufa y marinados chifa", Costo_Unitario = 6.00m, Estado = true, ID_Cat_Ingrediente = 8 },
                new Ingrediente { ID_Ingrediente = 30, Nombre_Ingrediente = "Comino Molido", Unidad_Medida = "kg", Descripcion = "Comino para aderezo de guisos y salsas criollas", Costo_Unitario = 12.00m, Estado = true, ID_Cat_Ingrediente = 8 },
                // Aceites y Grasas (cat 9)
                new Ingrediente { ID_Ingrediente = 31, Nombre_Ingrediente = "Aceite Vegetal", Unidad_Medida = "litro", Descripcion = "Aceite para freír papas y saltear ingredientes", Costo_Unitario = 7.00m, Estado = true, ID_Cat_Ingrediente = 9 },
                new Ingrediente { ID_Ingrediente = 32, Nombre_Ingrediente = "Mantequilla", Unidad_Medida = "kg", Descripcion = "Mantequilla para repostería y salsas cremosas", Costo_Unitario = 14.00m, Estado = true, ID_Cat_Ingrediente = 9 },
                // Harinas y Derivados (cat 10)
                new Ingrediente { ID_Ingrediente = 33, Nombre_Ingrediente = "Harina de Trigo", Unidad_Medida = "kg", Descripcion = "Harina para rebozados, masas y repostería", Costo_Unitario = 3.50m, Estado = true, ID_Cat_Ingrediente = 10 },
                new Ingrediente { ID_Ingrediente = 34, Nombre_Ingrediente = "Camote Amarillo", Unidad_Medida = "kg", Descripcion = "Camote para picarones y guarniciones de cuy", Costo_Unitario = 3.00m, Estado = true, ID_Cat_Ingrediente = 3 },
                new Ingrediente { ID_Ingrediente = 35, Nombre_Ingrediente = "Chicha de Jora", Unidad_Medida = "litro", Descripcion = "Chicha de jora para marinados y seco de res", Costo_Unitario = 5.00m, Estado = true, ID_Cat_Ingrediente = 8 }
                );

            base.OnModelCreating(modelBuilder);
        }
    }
}