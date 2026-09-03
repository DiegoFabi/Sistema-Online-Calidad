using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Services;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Controllers
{
    [Authorize(Roles = "Mesero")]
    public class MeseroController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public MeseroController(APPDBContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Mesas.OrderBy(m => m.Numero_Mesa);
            var resultado = await query.ToPagedListAsync(page, pageSize);

            var estadosOcupado = new[] { "Pendiente", "En Cocina", "Preparando", "Listo" };
            var mesasConPedido = await _dbcontext.Pedidos
                .Where(p => estadosOcupado.Contains(p.Estado_Pedido))
                .Select(p => p.ID_Mesa)
                .Distinct()
                .ToListAsync();

            var vm = new MeseroDashboardVM
            {
                Mesas = resultado.Items,
                MesasConPedidoActivo = new HashSet<int>(mesasConPedido),
                MesasLibres = resultado.Items.Count(m => !mesasConPedido.Contains(m.ID_Mesa)),
                MesasOcupadas = resultado.Items.Count(m => mesasConPedido.Contains(m.ID_Mesa)),
                PedidosActivos = mesasConPedido.Count
            };

            vm.PedidosListos = await _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Where(p => p.Estado_Pedido == "Listo")
                .ToListAsync();

            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(vm);
        }

        public async Task<IActionResult> Mesa(int id)
        {
            var mesa = await _dbcontext.Mesas.FirstOrDefaultAsync(m => m.ID_Mesa == id);
            if (mesa == null) return NotFound();

            var estadosActivos = new[] { "Pendiente", "En Cocina", "Preparando", "Listo", "Entregado" };
            var pedidos = await _dbcontext.Pedidos
                .Include(p => p.Empleado)
                .Include(p => p.Pedido_Detalles)
                    .ThenInclude(pd => pd.Producto)
                .Where(p => p.ID_Mesa == id && estadosActivos.Contains(p.Estado_Pedido))
                .OrderByDescending(p => p.ID_Pedido)
                .ToListAsync();

            ViewBag.Mesa = mesa;
            return View(pedidos);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarEntregado(int id)
        {
            var pedido = await _dbcontext.Pedidos
                .FirstOrDefaultAsync(p => p.ID_Pedido == id);
            if (pedido != null && pedido.Estado_Pedido == "Listo")
            {
                int mesaId = pedido.ID_Mesa;
                pedido.Estado_Pedido = "Entregado";
                await _dbcontext.SaveChangesAsync();

                // Liberar mesa si no quedan pedidos activos en ella
                var estadosOcupado = new[] { "Pendiente", "En Cocina", "Preparando", "Listo" };
                bool tieneOtrosActivos = await _dbcontext.Pedidos
                    .AnyAsync(p => p.ID_Mesa == mesaId && estadosOcupado.Contains(p.Estado_Pedido));
                if (!tieneOtrosActivos)
                {
                    var mesa = await _dbcontext.Mesas.FindAsync(mesaId);
                    if (mesa != null) { mesa.Estado = "Libre"; await _dbcontext.SaveChangesAsync(); }
                }

                NotificacionStore.Agregar("check_circle", "Pedido entregado", $"Pedido #{pedido.ID_Pedido} marcado como entregado. Listo para cobrar.");
                return RedirectToAction(nameof(Mesa), new { id = mesaId });
            }
            return RedirectToAction(nameof(Pedidos));
        }

        public async Task<IActionResult> Pedidos(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var estadosEnCurso = new[] { "Pendiente", "En Cocina", "Preparando", "Listo" };
            var query = _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Include(p => p.Pedido_Detalles)
                    .ThenInclude(pd => pd.Producto)
                .Where(p => estadosEnCurso.Contains(p.Estado_Pedido))
                .OrderByDescending(p => p.ID_Pedido);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        public async Task<IActionResult> Historial(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Include(p => p.Pedido_Detalles)
                    .ThenInclude(pd => pd.Producto)
                .Where(p => p.Estado_Pedido == "Entregado" || p.Estado_Pedido == "Pagado" || p.Estado_Pedido == "Cancelado")
                .OrderByDescending(p => p.ID_Pedido);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        public async Task<IActionResult> Notificaciones(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Where(p => p.Estado_Pedido == "Listo")
                .OrderByDescending(p => p.ID_Pedido);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }
    }
}