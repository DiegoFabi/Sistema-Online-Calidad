using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Services;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Controllers
{
    [Authorize(Roles = "Cajero")]
    public class CajeroController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public CajeroController(APPDBContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public async Task<IActionResult> Index(string orden = "desc")
        {
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);

            var vm = new CajeroDashboardVM
            {
                TotalRecaudadoHoy = await _dbcontext.Pagos
                    .Where(pg => pg.Fecha_Hora_Pago >= hoy && pg.Fecha_Hora_Pago < manana && pg.Estado == "Pagado")
                    .SumAsync(pg => (decimal?)pg.Monto) ?? 0,
                PagosHoyCount = await _dbcontext.Pagos.CountAsync(pg => pg.Fecha_Hora_Pago >= hoy && pg.Fecha_Hora_Pago < manana),
                PedidosPorCobrar = await _dbcontext.Pedidos.CountAsync(p => p.Estado_Pedido == "Entregado")
            };

            var queryEntregados = _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Where(p => p.Estado_Pedido == "Entregado");

            vm.PedidosPendientes = orden == "asc"
                ? await queryEntregados.OrderBy(p => p.ID_Pedido).Take(5).ToListAsync()
                : await queryEntregados.OrderByDescending(p => p.ID_Pedido).Take(5).ToListAsync();

            ViewBag.Orden = orden;

            vm.PagosRecientes = await _dbcontext.Pagos
                .Include(pg => pg.Pedido)
                .OrderByDescending(pg => pg.Fecha_Hora_Pago)
                .Take(6)
                .ToListAsync();

            vm.MetodosPago = await _dbcontext.Pagos
                .Where(pg => pg.Fecha_Hora_Pago >= hoy && pg.Fecha_Hora_Pago < manana)
                .GroupBy(pg => pg.Metodo_Pago)
                .Select(g => new MetodoPagoVM
                {
                    Metodo = g.Key,
                    Total = g.Sum(pg => pg.Monto),
                    Cantidad = g.Count()
                })
                .ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> Facturacion(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Where(p => p.Estado_Pedido == "Entregado" || p.Estado_Pedido == "Pagado")
                .OrderByDescending(p => p.ID_Pedido);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        public async Task<IActionResult> Pagos(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Pagos
                .Include(pg => pg.Pedido)
                .OrderByDescending(pg => pg.Fecha_Hora_Pago);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        public async Task<IActionResult> Clientes(string? buscar, int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Clientes.AsQueryable();
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(c => c.Nombre.Contains(buscar) || c.Apellidos.Contains(buscar) || c.DNI.Contains(buscar) || c.Email.Contains(buscar));
            }
            var resultado = await query.OrderBy(c => c.Nombre).ToPagedListAsync(page, pageSize);
            ViewBag.Buscar = buscar;
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

    }
}