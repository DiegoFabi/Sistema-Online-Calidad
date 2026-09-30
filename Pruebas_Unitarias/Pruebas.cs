using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SistemaOnline.Controllers;
using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;

namespace Pruebas_Unitarias
{
    internal static class Ayudante
    {
        private class SinTempData : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext c) => new Dictionary<string, object>();
            public void SaveTempData(HttpContext c, IDictionary<string, object> v) { }
        }

        public static APPDBContext NuevoContexto() =>
            new(new DbContextOptionsBuilder<APPDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        public static T Autenticar<T>(T controlador, string usuario, string rol) where T : Controller
        {
            var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, usuario), new Claim(ClaimTypes.Role, rol) }, "Test")) };
            controlador.ControllerContext = new ControllerContext { HttpContext = http };
            controlador.TempData = new TempDataDictionary(http, new SinTempData());
            return controlador;
        }
    }

    // Módulo: Mesas y flujo de pedidos
    public class MesasYFlujoDePedidosTests
    {
        // CP-01 - HU-05, esc. 3 — Cambio automático de estado al registrar un pedido
        [Test]
        public async Task CP01_CambioAutomaticoDeEstadoAlRegistrarPedido()
        {
            using var ctx = Ayudante.NuevoContexto();
            var carta = new Carta { Nombre_Carta = "Carta Principal", Cantidad_Platos = 1, Descripcion = "-", Precio = 1m };
            ctx.Cartas.Add(carta); ctx.SaveChanges();
            var categoria = new Producto_Categoria { Nombre_Categoria = "Platos de fondo", Descripcion = "-", ID_Carta = carta.ID_Carta };
            ctx.Productos_Categorias.Add(categoria); ctx.SaveChanges();
            var ceviche = new Producto { Nombre_Plato = "Ceviche clásico", Descripcion = "-", Tiempo_Preparacion = 15, Precio = 18.00m, Disponibilidad = true, ID_Categoria = categoria.ID_Categoria };
            ctx.Productos.Add(ceviche);
            var rolMesero = new Rol { Nombre_Rol = "Mesero", Descripcion = "-" };
            ctx.Roles.Add(rolMesero); ctx.SaveChanges();
            var usuario = new Usuario { Nombre_Usuario = "mesero1", Email = "mesero1@ranchosagrado.com", Password = "m3sero#2026", Estado = true, ID_Rol = rolMesero.ID_Rol };
            ctx.Usuarios.Add(usuario); ctx.SaveChanges();
            var empleado = new Empleado { Nombre = "Mesero", Apellidos = "Uno", Direccion = "-", Cargo = "Mesero", Estado = "Activo", DNI = "10000002", ID_Usuario = usuario.ID_Usuario };
            ctx.Empleados.Add(empleado);
            var mesa5 = new Mesa_Restaurante { Numero_Mesa = 5, Capacidad = 4, Ubicacion = "Salón", Estado = "Libre" };
            ctx.Mesas.Add(mesa5); ctx.SaveChanges();

            var controlador = Ayudante.Autenticar(new PedidoController(ctx), "mesero1", "Mesero");
            var modelo = new PedidoVM { ID_Mesa = mesa5.ID_Mesa, ID_Empleado = empleado.ID_Empleado, Estado_Pedido = "Pendiente", ProductosSeleccionados = new List<int> { ceviche.ID_Producto }, CantidadesProductos = new Dictionary<int, int> { { ceviche.ID_Producto, 2 } } };

            var resultado = await controlador.Nuevo(modelo);

            Assert.That(resultado, Is.InstanceOf<RedirectToActionResult>());
            var pedido = await ctx.Pedidos.SingleAsync(p => p.ID_Mesa == mesa5.ID_Mesa);
            Assert.That(pedido.SubTotal, Is.EqualTo(36.00m));
            Assert.That((await ctx.Mesas.FindAsync(mesa5.ID_Mesa))!.Estado, Is.EqualTo("Ocupada"));
        }

        // CP-02 - HU-05, esc. 1 — Intento de seleccionar una mesa ya ocupada
        [Test]
        public async Task CP02_NoSePuedeSeleccionarMesaOcupada()
        {
            using var ctx = Ayudante.NuevoContexto();
            var carta = new Carta { Nombre_Carta = "Carta Principal", Cantidad_Platos = 1, Descripcion = "-", Precio = 1m };
            ctx.Cartas.Add(carta); ctx.SaveChanges();
            var categoria = new Producto_Categoria { Nombre_Categoria = "Platos de fondo", Descripcion = "-", ID_Carta = carta.ID_Carta };
            ctx.Productos_Categorias.Add(categoria); ctx.SaveChanges();
            var lomoSaltado = new Producto { Nombre_Plato = "Lomo Saltado", Descripcion = "-", Tiempo_Preparacion = 20, Precio = 25.00m, Disponibilidad = true, ID_Categoria = categoria.ID_Categoria };
            ctx.Productos.Add(lomoSaltado);
            var empleado = new Empleado { Nombre = "Mesero", Apellidos = "Uno", Direccion = "-", Cargo = "Mesero", Estado = "Activo", DNI = "10000002" };
            ctx.Empleados.Add(empleado);
            var mesa3 = new Mesa_Restaurante { Numero_Mesa = 3, Capacidad = 4, Ubicacion = "Salón", Estado = "Ocupada" };
            ctx.Mesas.Add(mesa3); ctx.SaveChanges();
            var pedidoActivo = new Pedido { Fecha = DateTime.Now, Estado_Pedido = "Pendiente", SubTotal = 0, Total = 0, ID_Empleado = empleado.ID_Empleado, ID_Mesa = mesa3.ID_Mesa };
            ctx.Pedidos.Add(pedidoActivo); ctx.SaveChanges();

            var controlador = Ayudante.Autenticar(new PedidoController(ctx), "mesero1", "Mesero");
            var modelo = new PedidoVM { ID_Mesa = mesa3.ID_Mesa, ID_Empleado = empleado.ID_Empleado, Estado_Pedido = "Pendiente", ProductosSeleccionados = new List<int> { lomoSaltado.ID_Producto }, CantidadesProductos = new Dictionary<int, int> { { lomoSaltado.ID_Producto, 1 } } };

            var resultado = await controlador.Nuevo(modelo);

            Assert.That(resultado, Is.InstanceOf<ViewResult>());
            Assert.That(await ctx.Pedidos.CountAsync(p => p.ID_Mesa == mesa3.ID_Mesa), Is.EqualTo(1));
            Assert.That(await ctx.Pedidos_Detalles.CountAsync(), Is.EqualTo(0));
        }
    }

    // Módulo: Registro de pedidos (Mesero)
    public class RegistroDePedidosTests
    {
        // CP-03 - HU-07, esc. 1 — Registro exitoso de un pedido con uno o más productos
        [Test]
        public async Task CP03_RegistroExitosoDeUnPedidoConVariosProductos()
        {
            using var ctx = Ayudante.NuevoContexto();
            var carta = new Carta { Nombre_Carta = "Carta Principal", Cantidad_Platos = 1, Descripcion = "-", Precio = 1m };
            ctx.Cartas.Add(carta); ctx.SaveChanges();
            var categoria = new Producto_Categoria { Nombre_Categoria = "Platos de fondo", Descripcion = "-", ID_Carta = carta.ID_Carta };
            ctx.Productos_Categorias.Add(categoria); ctx.SaveChanges();
            var lomoSaltado = new Producto { Nombre_Plato = "Lomo Saltado", Descripcion = "-", Tiempo_Preparacion = 20, Precio = 25.00m, Disponibilidad = true, ID_Categoria = categoria.ID_Categoria };
            var chichaMorada = new Producto { Nombre_Plato = "Chicha morada", Descripcion = "-", Tiempo_Preparacion = 5, Precio = 6.00m, Disponibilidad = true, ID_Categoria = categoria.ID_Categoria };
            ctx.Productos.AddRange(lomoSaltado, chichaMorada);
            var rolMesero = new Rol { Nombre_Rol = "Mesero", Descripcion = "-" };
            ctx.Roles.Add(rolMesero); ctx.SaveChanges();
            var usuario = new Usuario { Nombre_Usuario = "mesero1", Email = "mesero1@ranchosagrado.com", Password = "m3sero#2026", Estado = true, ID_Rol = rolMesero.ID_Rol };
            ctx.Usuarios.Add(usuario); ctx.SaveChanges();
            var empleado = new Empleado { Nombre = "Mesero", Apellidos = "Uno", Direccion = "-", Cargo = "Mesero", Estado = "Activo", DNI = "10000002", ID_Usuario = usuario.ID_Usuario };
            ctx.Empleados.Add(empleado);
            var mesa2 = new Mesa_Restaurante { Numero_Mesa = 2, Capacidad = 4, Ubicacion = "Salón", Estado = "Libre" };
            ctx.Mesas.Add(mesa2); ctx.SaveChanges();

            var controlador = Ayudante.Autenticar(new PedidoController(ctx), "mesero1", "Mesero");
            var modelo = new PedidoVM
            {
                ID_Mesa = mesa2.ID_Mesa,
                ID_Empleado = empleado.ID_Empleado,
                Estado_Pedido = "Pendiente",
                ProductosSeleccionados = new List<int> { lomoSaltado.ID_Producto, chichaMorada.ID_Producto },
                CantidadesProductos = new Dictionary<int, int> { { lomoSaltado.ID_Producto, 2 }, { chichaMorada.ID_Producto, 1 } }
            };

            var resultado = await controlador.Nuevo(modelo);

            Assert.That(resultado, Is.InstanceOf<RedirectToActionResult>());
            var pedido = await ctx.Pedidos.SingleAsync(p => p.ID_Mesa == mesa2.ID_Mesa);
            Assert.That(pedido.SubTotal, Is.EqualTo(56.00m));
            Assert.That(await ctx.Pedidos_Detalles.CountAsync(pd => pd.ID_Pedido == pedido.ID_Pedido), Is.EqualTo(2));
        }

        // CP-04 - HU-07, esc. 2 — Intento de registrar un pedido sin productos seleccionados
        [Test]
        public async Task CP04_NoRegistraPedidoSinProductosSeleccionados()
        {
            using var ctx = Ayudante.NuevoContexto();
            var empleado = new Empleado { Nombre = "Mesero", Apellidos = "Uno", Direccion = "-", Cargo = "Mesero", Estado = "Activo", DNI = "10000002" };
            ctx.Empleados.Add(empleado);
            var mesa7 = new Mesa_Restaurante { Numero_Mesa = 7, Capacidad = 4, Ubicacion = "Salón", Estado = "Libre" };
            ctx.Mesas.Add(mesa7); ctx.SaveChanges();

            var controlador = Ayudante.Autenticar(new PedidoController(ctx), "mesero1", "Mesero");
            var modelo = new PedidoVM { ID_Mesa = mesa7.ID_Mesa, ID_Empleado = empleado.ID_Empleado, Estado_Pedido = "Pendiente", ProductosSeleccionados = new List<int>(), CantidadesProductos = new Dictionary<int, int>() };

            var resultado = await controlador.Nuevo(modelo);

            Assert.That(resultado, Is.InstanceOf<ViewResult>());
            Assert.That(await ctx.Pedidos.CountAsync(), Is.EqualTo(0));
            Assert.That((await ctx.Mesas.FindAsync(mesa7.ID_Mesa))!.Estado, Is.EqualTo("Libre"));
        }

        // CP-05 - HU-07, esc. 3 — Validación de stock disponible al registrar el pedido
        [Test]
        public async Task CP05_NoRegistraPedidoSiFaltaStockDeIngredientes()
        {
            using var ctx = Ayudante.NuevoContexto();
            var carta = new Carta { Nombre_Carta = "Carta Principal", Cantidad_Platos = 1, Descripcion = "-", Precio = 1m };
            ctx.Cartas.Add(carta); ctx.SaveChanges();
            var categoria = new Producto_Categoria { Nombre_Categoria = "Platos de fondo", Descripcion = "-", ID_Carta = carta.ID_Carta };
            ctx.Productos_Categorias.Add(categoria); ctx.SaveChanges();
            var pizza = new Producto { Nombre_Plato = "Pizza americana personal", Descripcion = "-", Tiempo_Preparacion = 15, Precio = 22.00m, Disponibilidad = true, ID_Categoria = categoria.ID_Categoria };
            ctx.Productos.Add(pizza); ctx.SaveChanges();
            var categoriaIng = new Categoria_Ingrediente { Nombre_Categoria = "Lácteos" };
            ctx.Categorias_Ingredientes.Add(categoriaIng); ctx.SaveChanges();
            var queso = new Ingrediente { Nombre_Ingrediente = "Queso mozzarella", Unidad_Medida = "kg", Descripcion = "-", Costo_Unitario = 18m, Estado = true, ID_Cat_Ingrediente = categoriaIng.ID_Cat_Ingrediente };
            ctx.Ingredientes.Add(queso); ctx.SaveChanges();
            ctx.Inventarios.Add(new Inventario { Cantidad_Stock = 0, Stock_Minimo = 5, Stock_Maximo = 50, Fecha_Ultima_Reposicion = DateTime.Now, ID_Ingrediente = queso.ID_Ingrediente });
            ctx.Productos_Ingredientes.Add(new Producto_Ingrediente { ID_Producto = pizza.ID_Producto, ID_Ingrediente = queso.ID_Ingrediente, Cantidad = 0.25m, Unidad_Medida = "kg", Observaciones = "-" });
            var empleado = new Empleado { Nombre = "Mesero", Apellidos = "Uno", Direccion = "-", Cargo = "Mesero", Estado = "Activo", DNI = "10000002" };
            ctx.Empleados.Add(empleado);
            var mesa6 = new Mesa_Restaurante { Numero_Mesa = 6, Capacidad = 4, Ubicacion = "Salón", Estado = "Libre" };
            ctx.Mesas.Add(mesa6); ctx.SaveChanges();

            var controlador = Ayudante.Autenticar(new PedidoController(ctx), "mesero1", "Mesero");
            var modelo = new PedidoVM { ID_Mesa = mesa6.ID_Mesa, ID_Empleado = empleado.ID_Empleado, Estado_Pedido = "Pendiente", ProductosSeleccionados = new List<int> { pizza.ID_Producto }, CantidadesProductos = new Dictionary<int, int> { { pizza.ID_Producto, 1 } } };

            var resultado = await controlador.Nuevo(modelo);

            Assert.That(resultado, Is.InstanceOf<ViewResult>());
            Assert.That(await ctx.Pedidos.CountAsync(), Is.EqualTo(0));
            Assert.That((await ctx.Mesas.FindAsync(mesa6.ID_Mesa))!.Estado, Is.EqualTo("Libre"));
        }
    }

    // Módulo: Preparación del pedido (Cocina)
    public class PreparacionDelPedidoTests
    {
        // CP-06 - HU-08, esc. 3 — Avance del estado de un pedido
        [Test]
        public async Task CP06_AvanceDeEstadoDeUnPedido()
        {
            using var ctx = Ayudante.NuevoContexto();
            var empleado = new Empleado { Nombre = "Mesero", Apellidos = "Uno", Direccion = "-", Cargo = "Mesero", Estado = "Activo", DNI = "10000002" };
            ctx.Empleados.Add(empleado);
            var mesa5 = new Mesa_Restaurante { Numero_Mesa = 5, Capacidad = 4, Ubicacion = "Salón", Estado = "Ocupada" };
            ctx.Mesas.Add(mesa5); ctx.SaveChanges();
            var pedido101 = new Pedido { Fecha = DateTime.Now, Estado_Pedido = "Pendiente", SubTotal = 25m, Total = 29.5m, ID_Empleado = empleado.ID_Empleado, ID_Mesa = mesa5.ID_Mesa };
            ctx.Pedidos.Add(pedido101); ctx.SaveChanges();

            var controlador = new CocinaController(ctx);

            await controlador.AvanzarEstado(pedido101.ID_Pedido, "Preparando");
            Assert.That((await ctx.Pedidos.FindAsync(pedido101.ID_Pedido))!.Estado_Pedido, Is.EqualTo("Preparando"));

            await controlador.AvanzarEstado(pedido101.ID_Pedido, "Listo");
            Assert.That((await ctx.Pedidos.FindAsync(pedido101.ID_Pedido))!.Estado_Pedido, Is.EqualTo("Listo"));
        }
    }

    // Módulo: Cobro del pedido (Cajero)
    public class CobroDelPedidoTests
    {
        // CP-07 - HU-16, esc. 1 — Registro exitoso de un pago
        [Test]
        public async Task CP07_RegistroExitosoDeUnPago()
        {
            using var ctx = Ayudante.NuevoContexto();
            var empleado = new Empleado { Nombre = "Cajero", Apellidos = "Uno", Direccion = "-", Cargo = "Cajero", Estado = "Activo", DNI = "10000001" };
            ctx.Empleados.Add(empleado);
            var mesa5 = new Mesa_Restaurante { Numero_Mesa = 5, Capacidad = 4, Ubicacion = "Salón", Estado = "Ocupada" };
            ctx.Mesas.Add(mesa5); ctx.SaveChanges();
            var pedido = new Pedido { Fecha = DateTime.Now, Estado_Pedido = "Listo", SubTotal = 45m, Total = 45m, ID_Empleado = empleado.ID_Empleado, ID_Mesa = mesa5.ID_Mesa };
            ctx.Pedidos.Add(pedido); ctx.SaveChanges();

            var controlador = Ayudante.Autenticar(new PagoController(ctx), "cajero1", "Cajero");
            var modelo = new PagoVM { Fecha_Hora_Pago = DateTime.Now, Monto = 45.00m, Metodo_Pago = "Efectivo", Estado = "Pagado", ID_Pedido = pedido.ID_Pedido };

            var resultado = await controlador.Nuevo(modelo);

            Assert.That(resultado, Is.InstanceOf<RedirectToActionResult>());
            var pago = await ctx.Pagos.SingleAsync(p => p.ID_Pedido == pedido.ID_Pedido);
            Assert.That(pago.Monto, Is.EqualTo(45.00m));
            Assert.That(pago.Metodo_Pago, Is.EqualTo("Efectivo"));
        }

        // CP-08 - HU-16, esc. 2 — Intento de registrar un pago con monto igual a cero
        [Test]
        public async Task CP08_NoRegistraPagoConMontoIgualACero()
        {
            using var ctx = Ayudante.NuevoContexto();
            var empleado = new Empleado { Nombre = "Cajero", Apellidos = "Uno", Direccion = "-", Cargo = "Cajero", Estado = "Activo", DNI = "10000001" };
            ctx.Empleados.Add(empleado);
            var mesa7 = new Mesa_Restaurante { Numero_Mesa = 7, Capacidad = 4, Ubicacion = "Salón", Estado = "Ocupada" };
            ctx.Mesas.Add(mesa7); ctx.SaveChanges();
            var pedido = new Pedido { Fecha = DateTime.Now, Estado_Pedido = "Listo", SubTotal = 32m, Total = 32m, ID_Empleado = empleado.ID_Empleado, ID_Mesa = mesa7.ID_Mesa };
            ctx.Pedidos.Add(pedido); ctx.SaveChanges();

            var controlador = Ayudante.Autenticar(new PagoController(ctx), "cajero1", "Cajero");
            var modelo = new PagoVM { Fecha_Hora_Pago = DateTime.Now, Monto = 0.00m, Metodo_Pago = "Efectivo", Estado = "Pendiente", ID_Pedido = pedido.ID_Pedido };
            controlador.ModelState.AddModelError("Monto", "El monto debe ser mayor o igual a 1.");

            var resultado = await controlador.Nuevo(modelo);

            Assert.That(resultado, Is.InstanceOf<ViewResult>());
            Assert.That(await ctx.Pagos.CountAsync(p => p.ID_Pedido == pedido.ID_Pedido), Is.EqualTo(0));
        }

        // CP-09 - HU-16, esc. 3 — Prevención del doble cobro de un mismo pedido
        [Test]
        public async Task CP09_PrevieneDobleCobroDelMismoPedido()
        {
            using var ctx = Ayudante.NuevoContexto();
            var empleado = new Empleado { Nombre = "Cajero", Apellidos = "Uno", Direccion = "-", Cargo = "Cajero", Estado = "Activo", DNI = "10000001" };
            ctx.Empleados.Add(empleado);
            var mesa5 = new Mesa_Restaurante { Numero_Mesa = 5, Capacidad = 4, Ubicacion = "Salón", Estado = "Ocupada" };
            ctx.Mesas.Add(mesa5); ctx.SaveChanges();
            var pedido = new Pedido { Fecha = DateTime.Now, Estado_Pedido = "Listo", SubTotal = 45m, Total = 45m, ID_Empleado = empleado.ID_Empleado, ID_Mesa = mesa5.ID_Mesa };
            ctx.Pedidos.Add(pedido); ctx.SaveChanges();

            var primerCobro = Ayudante.Autenticar(new PagoController(ctx), "cajero1", "Cajero");
            await primerCobro.Nuevo(new PagoVM { Fecha_Hora_Pago = DateTime.Now, Monto = 45.00m, Metodo_Pago = "Efectivo", Estado = "Pagado", ID_Pedido = pedido.ID_Pedido });

            var segundoCobro = Ayudante.Autenticar(new PagoController(ctx), "cajero1", "Cajero");
            var resultado = await segundoCobro.Nuevo(new PagoVM { Fecha_Hora_Pago = DateTime.Now, Monto = 45.00m, Metodo_Pago = "Yape", Estado = "Pagado", ID_Pedido = pedido.ID_Pedido });

            Assert.That(resultado, Is.InstanceOf<ViewResult>());
            Assert.That(await ctx.Pagos.CountAsync(p => p.ID_Pedido == pedido.ID_Pedido), Is.EqualTo(1));
        }
    }
}
