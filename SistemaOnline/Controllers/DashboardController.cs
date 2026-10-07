using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.Services;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DashboardController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public DashboardController(APPDBContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;

            var vm = new AdminDashboardVM
            {
                VentasTotales = await _dbcontext.Pedidos.SumAsync(p => (decimal?)p.Total) ?? 0,
                VentasHoy = await _dbcontext.Pedidos.Where(p => p.Fecha == hoy).SumAsync(p => (decimal?)p.Total) ?? 0,
                PedidosActivos = await _dbcontext.Pedidos.CountAsync(p => p.Estado_Pedido != "Completado" && p.Estado_Pedido != "Pagado" && p.Estado_Pedido != "Cancelado"),
                ClientesTotales = await _dbcontext.Clientes.CountAsync(),
                AlertasInventario = await _dbcontext.Inventarios.CountAsync(i => i.Cantidad_Stock <= i.Stock_Minimo)
            };

            vm.InventarioCritico = await _dbcontext.Inventarios
                .Include(i => i.Ingrediente)
                .Where(i => i.Cantidad_Stock <= i.Stock_Minimo)
                .OrderBy(i => i.Cantidad_Stock)
                .Take(5)
                .ToListAsync();

            vm.PedidosRecientes = await _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .OrderByDescending(p => p.ID_Pedido)
                .Take(5)
                .ToListAsync();

            vm.TopEmpleados = await _dbcontext.Pedidos
                .Include(p => p.Empleado)
                .Where(p => p.Empleado != null)
                .GroupBy(p => new { p.ID_Empleado, p.Empleado.Nombre, p.Empleado.Apellidos })
                .Select(g => new EmpleadoVentaVM
                {
                    NombreCompleto = g.Key.Nombre + " " + g.Key.Apellidos,
                    TotalVendido = g.Sum(p => p.Total),
                    CantidadPedidos = g.Count()
                })
                .OrderByDescending(e => e.TotalVendido)
                .Take(3)
                .ToListAsync();

            var ventasAgrupadas = await _dbcontext.Pedidos
                .Where(p => p.Fecha >= hoy.AddMonths(-5))
                .GroupBy(p => new { p.Fecha.Year, p.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(p => p.Total) })
                .ToListAsync();

            var culturaEs = new System.Globalization.CultureInfo("es-PE");
            vm.VentasPorMes = Enumerable.Range(0, 6)
                .Select(i => hoy.AddMonths(-5 + i))
                .Select(fecha =>
                {
                    var datoMes = ventasAgrupadas.FirstOrDefault(v => v.Year == fecha.Year && v.Month == fecha.Month);
                    return new VentaMensualVM
                    {
                        Mes = culturaEs.DateTimeFormat.GetAbbreviatedMonthName(fecha.Month),
                        Total = datoMes?.Total ?? 0
                    };
                })
                .ToList();

            var coloresPorEstado = new Dictionary<string, string>
            {
                ["Pendiente"] = "#f5a623",
                ["Servido"] = "#4f9d69",
                ["Pagado"] = "#3b7dd8",
                ["Cancelado"] = "#d8463b"
            };

            var pedidosAgrupados = await _dbcontext.Pedidos
                .GroupBy(p => p.Estado_Pedido)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            vm.PedidosPorEstado = pedidosAgrupados
                .Select(g => new EstadoPedidoVM
                {
                    Estado = g.Estado,
                    Cantidad = g.Cantidad,
                    Color = coloresPorEstado.TryGetValue(g.Estado, out var color) ? color : "#9aa0a6"
                })
                .ToList();

            return View(vm);
        }

        public async Task<IActionResult> Usuarios(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Usuarios.Include(u => u.Rol).OrderBy(u => u.ID_Usuario);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            ViewBag.RolesDisponibles = await _dbcontext.Roles.Select(r => new SelectListItem
            {
                Value = r.ID_Rol.ToString(),
                Text = r.Nombre_Rol
            }).ToListAsync();
            return View(resultado.Items);
        }

        // Edicion completa de un usuario desde el boton de editar de la lista (modal).
        // Misma logica que UsuarioController.Editar, pero vuelve a Dashboard/Usuarios.
        [HttpPost]
        public async Task<IActionResult> EditarUsuario(UsuarioVM modelo)
        {
            Usuario? usuario = await _dbcontext.Usuarios.FirstOrDefaultAsync(u => u.ID_Usuario == modelo.ID_Usuario);
            if (usuario == null || !await _dbcontext.Roles.AnyAsync(r => r.ID_Rol == modelo.ID_Rol))
            {
                TempData["Error"] = "No se pudo actualizar el usuario: datos inválidos.";
                return RedirectToAction(nameof(Usuarios));
            }

            int rolAnteriorId = usuario.ID_Rol;
            bool estadoAnterior = usuario.Estado;
            usuario.Nombre_Usuario = modelo.Nombre_Usuario;
            usuario.Email = modelo.Email;
            usuario.Password = modelo.Password;
            usuario.Estado = modelo.Estado;
            usuario.ID_Rol = modelo.ID_Rol;
            await _dbcontext.SaveChangesAsync();

            var detalles = new List<string>();
            if (rolAnteriorId != modelo.ID_Rol)
            {
                var rolAnterior = await _dbcontext.Roles.FindAsync(rolAnteriorId);
                var rolNuevo = await _dbcontext.Roles.FindAsync(modelo.ID_Rol);
                detalles.Add($"rol '{rolAnterior?.Nombre_Rol}' → '{rolNuevo?.Nombre_Rol}'");
            }
            if (estadoAnterior != modelo.Estado)
            {
                detalles.Add($"estado {(estadoAnterior ? "Activo" : "Inactivo")} → {(modelo.Estado ? "Activo" : "Inactivo")}");
            }
            string detalleCambios = detalles.Any() ? string.Join("; ", detalles) : "datos generales actualizados";
            AuditoriaStore.Registrar(User, "Edición", "Usuario",
                $"Se editó el usuario '{usuario.Nombre_Usuario}' ({usuario.Email}): {detalleCambios}.");

            TempData["Exito"] = "El usuario se actualizó correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        public async Task<IActionResult> Inventario(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Inventarios.Include(i => i.Ingrediente).OrderBy(i => i.ID_Inventario);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        public async Task<IActionResult> Reportes(int? categoria, string? metodoPago, int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Pedidos.AsQueryable();

            if (categoria.HasValue)
            {
                query = query.Where(p => p.Pedido_Detalles.Any(pd => pd.Producto.ID_Categoria == categoria.Value));
            }

            if (!string.IsNullOrWhiteSpace(metodoPago))
            {
                query = query.Where(p => p.Pagos.Any(pg => pg.Metodo_Pago == metodoPago));
            }

            query = query.OrderByDescending(p => p.Fecha);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;

            ViewBag.CategoriaSeleccionada = categoria;
            ViewBag.MetodoPagoSeleccionado = metodoPago;
            ViewBag.CategoriasDisponibles = await _dbcontext.Productos_Categorias.OrderBy(c => c.Nombre_Categoria).ToListAsync();
            ViewBag.MetodosPagoDisponibles = await _dbcontext.Pagos.Select(pg => pg.Metodo_Pago).Distinct().OrderBy(m => m).ToListAsync();

            return View(resultado.Items);
        }
    }
}