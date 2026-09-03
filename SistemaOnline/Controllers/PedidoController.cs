using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class PedidoController : Controller
    {
        private readonly APPDBContext _context;
        public PedidoController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Pedidos
                .Include(p => p.Empleado)
                .Include(p => p.Mesa_Restaurante)
                .OrderBy(p => p.ID_Pedido)
                .Select(p => new PedidoVM
                {
                    ID_Pedido = p.ID_Pedido,
                    Fecha = p.Fecha,
                    Estado_Pedido = p.Estado_Pedido,
                    Detalle_Pedido = p.Detalle_Pedido,
                    SubTotal = p.SubTotal,
                    Total = p.Total,
                    ID_Empleado = p.ID_Empleado,
                    ID_Mesa = p.ID_Mesa,
                    EmpleadoNombre = $"{p.Empleado.Nombre} {p.Empleado.Apellidos}",
                    MesaNumero = p.Mesa_Restaurante.Numero_Mesa.ToString()
                });

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo(int? mesaId = null)
        {
            if (!await _context.Empleados.AnyAsync() || !await _context.Mesas.AnyAsync())
            {
                if (User.IsInRole("Administrador"))
                {
                    ViewData["Msg"] = "Debes registrar al menos un Empleado y una Mesa antes de crear un Pedido.";
                    return View("~/Views/Negocio/Advertencia.cshtml");
                }
                return View("~/Views/Shared/SistemaNoConfigurado.cshtml");
            }

            bool esMesero = User.IsInRole("Mesero");
            int preselEmpleado = 0;
            List<SelectListItem> empleadosList;

            if (esMesero)
            {
                // Pre-select the authenticated mesero's Empleado record
                var emailUsuario = User.Identity?.Name;
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailUsuario || u.Nombre_Usuario == emailUsuario);
                var empleado = usuario != null
                    ? await _context.Empleados.FirstOrDefaultAsync(e => e.ID_Usuario == usuario.ID_Usuario)
                    : null;
                preselEmpleado = empleado?.ID_Empleado ?? 0;
                empleadosList = empleado != null
                    ? new List<SelectListItem> { new SelectListItem { Value = empleado.ID_Empleado.ToString(), Text = $"{empleado.Nombre} {empleado.Apellidos}", Selected = true } }
                    : new List<SelectListItem>();
            }
            else
            {
                // Admin: show only Mesero-role employees
                empleadosList = await ObtenerEmpleadosMeseros();
            }

            PedidoVM modelo = new PedidoVM
            {
                Fecha = DateTime.Now,
                Estado_Pedido = esMesero ? "En Cocina" : "Pendiente",
                ID_Empleado = preselEmpleado,
                ID_Mesa = mesaId ?? 0,
                EmpleadosDisponibles = empleadosList,
                MesasDisponibles = await ObtenerMesas(),
                CategoriasProductos = await ObtenerCategoriasProductos()
            };
            ViewBag.MesasActivas = await ObtenerMesasConPedidoActivo();
            ViewBag.EsMesero = esMesero;
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(PedidoVM modelo)
        {
            bool esMesero = User.IsInRole("Mesero");

            if (esMesero)
            {
                // Enforce mesero's own employee — reject any tampered ID
                var emailUsuario = User.Identity?.Name;
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == emailUsuario || u.Nombre_Usuario == emailUsuario);
                var empleado = usuario != null ? await _context.Empleados.FirstOrDefaultAsync(e => e.ID_Usuario == usuario.ID_Usuario) : null;
                if (empleado == null)
                    ModelState.AddModelError(nameof(modelo.ID_Empleado), "No se encontró tu registro de empleado.");
                else
                    modelo.ID_Empleado = empleado.ID_Empleado;
                modelo.Estado_Pedido = "En Cocina";
            }
            else
            {
                if (!await _context.Empleados.AnyAsync(e => e.ID_Empleado == modelo.ID_Empleado))
                    ModelState.AddModelError(nameof(modelo.ID_Empleado), "Selecciona un empleado válido.");
            }

            if (!await _context.Mesas.AnyAsync(m => m.ID_Mesa == modelo.ID_Mesa))
                ModelState.AddModelError(nameof(modelo.ID_Mesa), "Selecciona una mesa válida.");

            if (modelo.ProductosSeleccionados == null || !modelo.ProductosSeleccionados.Any())
                ModelState.AddModelError(nameof(modelo.ProductosSeleccionados), "Debes seleccionar al menos un producto.");

            if (!ModelState.IsValid)
            {
                modelo.EmpleadosDisponibles = esMesero ? new List<SelectListItem>() : await ObtenerEmpleadosMeseros();
                modelo.MesasDisponibles = await ObtenerMesas();
                modelo.CategoriasProductos = await ObtenerCategoriasProductos();
                ViewBag.MesasActivas = await ObtenerMesasConPedidoActivo();
                ViewBag.EsMesero = esMesero;
                return View(modelo);
            }

            // Recalcular totales desde productos seleccionados para evitar manipulación client-side
            decimal subTotalCalculado = 0;
            if (modelo.ProductosSeleccionados != null && modelo.ProductosSeleccionados.Any())
            {
                var precios = await _context.Productos
                    .Where(p => modelo.ProductosSeleccionados.Contains(p.ID_Producto))
                    .ToDictionaryAsync(p => p.ID_Producto, p => p.Precio);
                foreach (var idProd in modelo.ProductosSeleccionados)
                {
                    int qty = modelo.CantidadesProductos != null && modelo.CantidadesProductos.TryGetValue(idProd, out int q) && q > 0 ? q : 1;
                    if (precios.TryGetValue(idProd, out decimal precio)) subTotalCalculado += precio * qty;
                }
            }
            decimal igv = Math.Round(subTotalCalculado * 0.18m, 2);

            Pedido pedido = new Pedido
            {
                Fecha = DateTime.Now,
                Estado_Pedido = modelo.Estado_Pedido,
                Detalle_Pedido = modelo.Detalle_Pedido,
                SubTotal = subTotalCalculado,
                Total = subTotalCalculado + igv,
                ID_Empleado = modelo.ID_Empleado,
                ID_Mesa = modelo.ID_Mesa
            };
            await _context.Pedidos.AddAsync(pedido);
            await _context.SaveChangesAsync();

            // Marcar mesa como Ocupada
            var mesaParaOcupar = await _context.Mesas.FindAsync(pedido.ID_Mesa);
            if (mesaParaOcupar != null && mesaParaOcupar.Estado != "Ocupada")
            {
                mesaParaOcupar.Estado = "Ocupada";
                await _context.SaveChangesAsync();
            }

            if (User.IsInRole("Mesero"))
                Services.NotificacionStore.Agregar("restaurant_menu", "Nuevo pedido en cocina", $"Pedido #{pedido.ID_Pedido} enviado a cocina — Mesa {pedido.ID_Mesa}.");
            else
                Services.NotificacionStore.Agregar("receipt_long", "Pedido creado", $"Nuevo pedido #{pedido.ID_Pedido} registrado.");

            if (modelo.ProductosSeleccionados != null && modelo.ProductosSeleccionados.Any())
            {
                var productos = await _context.Productos
                    .Where(p => modelo.ProductosSeleccionados.Contains(p.ID_Producto))
                    .ToListAsync();

                foreach (var producto in productos)
                {
                    int cantidad = modelo.CantidadesProductos != null && modelo.CantidadesProductos.TryGetValue(producto.ID_Producto, out int qty) && qty > 0 ? qty : 1;
                    await _context.Pedidos_Detalles.AddAsync(new Pedido_Detalle
                    {
                        ID_Pedido = pedido.ID_Pedido,
                        ID_Producto = producto.ID_Producto,
                        Cantidad = cantidad,
                        PrecioUnitario = producto.Precio
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["Confetti"] = true;
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Pedido pedido = await _context.Pedidos
                .Include(p => p.Pedido_Detalles)
                .FirstAsync(p => p.ID_Pedido == id);
            PedidoVM modelo = new PedidoVM
            {
                ID_Pedido = pedido.ID_Pedido,
                Fecha = pedido.Fecha,
                Estado_Pedido = pedido.Estado_Pedido,
                Detalle_Pedido = pedido.Detalle_Pedido,
                SubTotal = pedido.SubTotal,
                Total = pedido.Total,
                ID_Empleado = pedido.ID_Empleado,
                ID_Mesa = pedido.ID_Mesa,
                ProductosSeleccionados = pedido.Pedido_Detalles?.Select(pd => pd.ID_Producto).ToList() ?? new List<int>(),
                CantidadesProductos = pedido.Pedido_Detalles?.ToDictionary(pd => pd.ID_Producto, pd => pd.Cantidad) ?? new Dictionary<int, int>(),
                EmpleadosDisponibles = await ObtenerEmpleados(),
                MesasDisponibles = await ObtenerMesas(),
                CategoriasProductos = await ObtenerCategoriasProductos()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(PedidoVM modelo)
        {
            if (!await _context.Empleados.AnyAsync(e => e.ID_Empleado == modelo.ID_Empleado))
                ModelState.AddModelError(nameof(modelo.ID_Empleado), "Selecciona un empleado válido.");
            if (!await _context.Mesas.AnyAsync(m => m.ID_Mesa == modelo.ID_Mesa))
                ModelState.AddModelError(nameof(modelo.ID_Mesa), "Selecciona una mesa válida.");

            if (!ModelState.IsValid)
            {
                modelo.EmpleadosDisponibles = await ObtenerEmpleados();
                modelo.MesasDisponibles = await ObtenerMesas();
                modelo.CategoriasProductos = await ObtenerCategoriasProductos();
                return View(modelo);
            }

            // Recalcular totales desde productos seleccionados para evitar manipulación client-side
            decimal subTotalEditado = 0;
            if (modelo.ProductosSeleccionados != null && modelo.ProductosSeleccionados.Any())
            {
                var precios = await _context.Productos
                    .Where(p => modelo.ProductosSeleccionados.Contains(p.ID_Producto))
                    .ToDictionaryAsync(p => p.ID_Producto, p => p.Precio);
                foreach (var idProd in modelo.ProductosSeleccionados)
                {
                    int qty = modelo.CantidadesProductos != null && modelo.CantidadesProductos.TryGetValue(idProd, out int q) && q > 0 ? q : 1;
                    if (precios.TryGetValue(idProd, out decimal precio)) subTotalEditado += precio * qty;
                }
            }
            decimal igvEditado = Math.Round(subTotalEditado * 0.18m, 2);

            Pedido pedido = await _context.Pedidos
                .Include(p => p.Pedido_Detalles)
                .FirstAsync(p => p.ID_Pedido == modelo.ID_Pedido);
            pedido.Fecha = modelo.Fecha;
            pedido.Estado_Pedido = modelo.Estado_Pedido;
            pedido.Detalle_Pedido = modelo.Detalle_Pedido;
            pedido.SubTotal = subTotalEditado;
            pedido.Total = subTotalEditado + igvEditado;
            pedido.ID_Empleado = modelo.ID_Empleado;
            pedido.ID_Mesa = modelo.ID_Mesa;
            _context.Pedidos.Update(pedido);

            // Reemplaza los detalles del pedido con los productos seleccionados en el modal
            if (pedido.Pedido_Detalles != null && pedido.Pedido_Detalles.Any())
            {
                _context.Pedidos_Detalles.RemoveRange(pedido.Pedido_Detalles);
            }

            if (modelo.ProductosSeleccionados != null && modelo.ProductosSeleccionados.Any())
            {
                var productos = await _context.Productos
                    .Where(p => modelo.ProductosSeleccionados.Contains(p.ID_Producto))
                    .ToListAsync();

                foreach (var producto in productos)
                {
                    int cantidad = modelo.CantidadesProductos != null && modelo.CantidadesProductos.TryGetValue(producto.ID_Producto, out int qty) && qty > 0 ? qty : 1;
                    await _context.Pedidos_Detalles.AddAsync(new Pedido_Detalle
                    {
                        ID_Pedido = pedido.ID_Pedido,
                        ID_Producto = producto.ID_Producto,
                        Cantidad = cantidad,
                        PrecioUnitario = producto.Precio
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            var tienePago = await _context.Pagos.AnyAsync(p => p.ID_Pedido == id);
            var tieneComprobante = await _context.Comprobantes_Pagos.AnyAsync(c => c.ID_Pedido == id);
            if (tienePago || tieneComprobante)
            {
                TempData["Error"] = "No se puede eliminar un pedido que tiene pagos o comprobantes asociados.";
                return RedirectToAction(nameof(Lista));
            }

            Pedido pedido = await _context.Pedidos.FirstAsync(p => p.ID_Pedido == id);
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<int>> ObtenerMesasConPedidoActivo()
        {
            return await _context.Pedidos
                .Where(p => p.Estado_Pedido != "Pagado" && p.Estado_Pedido != "Cancelado")
                .Select(p => p.ID_Mesa)
                .Distinct()
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> ObtenerEmpleados()
        {
            var lista = await _context.Empleados.Select(e => new SelectListItem
            {
                Value = e.ID_Empleado.ToString(),
                Text = $"{e.Nombre} {e.Apellidos}"
            }).ToListAsync();
            return lista;
        }

        private async Task<List<SelectListItem>> ObtenerEmpleadosMeseros()
        {
            var lista = await _context.Empleados
                .Where(e => e.Cargo == "Mesero")
                .Select(e => new SelectListItem
                {
                    Value = e.ID_Empleado.ToString(),
                    Text = $"{e.Nombre} {e.Apellidos}"
                }).ToListAsync();
            return lista;
        }

        private async Task<List<SelectListItem>> ObtenerMesas()
        {
            var mesas = await _context.Mesas
                .OrderBy(m => m.Numero_Mesa)
                .ToListAsync();

            // IDs con pedidos activos para mostrar indicador en el label
            var estadosOcupado = new[] { "Pendiente", "En Cocina", "Preparando", "Listo" };
            var mesasOcupadas = await _context.Pedidos
                .Where(p => estadosOcupado.Contains(p.Estado_Pedido))
                .Select(p => p.ID_Mesa)
                .Distinct()
                .ToListAsync();
            var ocupadasSet = new HashSet<int>(mesasOcupadas);

            return mesas.Select(m => new SelectListItem
            {
                Value = m.ID_Mesa.ToString(),
                Text = ocupadasSet.Contains(m.ID_Mesa)
                    ? $"Mesa {m.Numero_Mesa} ({m.Ubicacion}) — Ocupada"
                    : $"Mesa {m.Numero_Mesa} ({m.Ubicacion})"
            }).ToList();
        }

        private async Task<List<CategoriaProductosVM>> ObtenerCategoriasProductos()
        {
            var categorias = await _context.Productos_Categorias
                .Include(c => c.Productos)
                .ToListAsync();

            return categorias.Select(c => new CategoriaProductosVM
            {
                ID_Categoria = c.ID_Categoria,
                Nombre_Categoria = c.Nombre_Categoria,
                Productos = (c.Productos ?? new List<Producto>())
                    .Select(p => new ProductoOpcionVM
                    {
                        ID_Producto = p.ID_Producto,
                        Nombre_Plato = p.Nombre_Plato,
                        Precio = p.Precio
                    }).ToList()
            }).ToList();
        }
    }
}