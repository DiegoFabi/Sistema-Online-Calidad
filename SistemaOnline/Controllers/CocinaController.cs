using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Services;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Controllers
{
    [Authorize(Roles = "Chef")]
    public class CocinaController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public CocinaController(APPDBContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var pedidos = await _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Include(p => p.Pedido_Detalles)
                    .ThenInclude(pd => pd.Producto)
                .Where(p => p.Estado_Pedido == "Pendiente" || p.Estado_Pedido == "En Cocina" || p.Estado_Pedido == "Preparando" || p.Estado_Pedido == "Listo")
                .OrderBy(p => p.ID_Pedido)
                .ToListAsync();

            var vm = new CocinaDashboardVM
            {
                Pendientes = pedidos.Where(p => p.Estado_Pedido == "Pendiente" || p.Estado_Pedido == "En Cocina").ToList(),
                Preparando = pedidos.Where(p => p.Estado_Pedido == "Preparando").ToList(),
                Listos = pedidos.Where(p => p.Estado_Pedido == "Listo").ToList(),
                AlertasInventario = await _dbcontext.Inventarios.CountAsync(i => i.Cantidad_Stock <= i.Stock_Minimo)
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AvanzarEstado(int id, string nuevoEstado)
        {
            var pedido = await _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .FirstOrDefaultAsync(p => p.ID_Pedido == id);
            if (pedido != null)
            {
                var estadosValidos = new[] { "En Cocina", "Preparando", "Listo" };
                if (estadosValidos.Contains(nuevoEstado))
                {
                    pedido.Estado_Pedido = nuevoEstado;
                    await _dbcontext.SaveChangesAsync();

                    if (nuevoEstado == "Listo")
                    {
                        var mesa = pedido.Mesa_Restaurante != null ? $"Mesa {pedido.Mesa_Restaurante.Numero_Mesa}" : $"Mesa {pedido.ID_Mesa}";
                        NotificacionStore.Agregar("notifications_active", "Pedido listo para servir", $"Pedido #{pedido.ID_Pedido} ({mesa}) está listo. El mesero debe recogerlo.");
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Ingredientes(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Inventarios
                .Include(i => i.Ingrediente)
                    .ThenInclude(ing => ing.Categoria_Ingrediente)
                .OrderBy(i => i.Ingrediente.Nombre_Ingrediente);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        public async Task<IActionResult> Detalle(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Include(p => p.Pedido_Detalles)
                    .ThenInclude(pd => pd.Producto)
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
