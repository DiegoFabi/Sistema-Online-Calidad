using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Services;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministradorController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public AdministradorController(APPDBContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);
            var limiteContrato = hoy.AddDays(30);

            var vm = new AdminDashboardVM
            {
                ReservacionesHoy = await _dbcontext.Reservaciones.CountAsync(r => r.Fecha_Hora >= hoy && r.Fecha_Hora < manana),
                PedidosActivos = await _dbcontext.Pedidos.CountAsync(p => p.Estado_Pedido != "Completado" && p.Estado_Pedido != "Pagado" && p.Estado_Pedido != "Cancelado"),
                MesasOcupadas = await _dbcontext.Mesas.CountAsync(m => m.Estado == "Ocupada"),
                ContratosPorVencer = await _dbcontext.Contratos.CountAsync(c => c.Fecha_Fin >= hoy && c.Fecha_Fin <= limiteContrato)
            };

            vm.ProximasReservaciones = await _dbcontext.Reservaciones
                .Include(r => r.Cliente)
                .Include(r => r.Mesa_Restaurante)
                .Where(r => r.Fecha_Hora >= hoy)
                .OrderBy(r => r.Fecha_Hora)
                .Take(6)
                .ToListAsync();

            vm.Turnos = await _dbcontext.Turnos
                .Include(t => t.Empleado_Turnos)
                    .ThenInclude(et => et.Empleado)
                .ToListAsync();

            vm.AlertasContratos = await _dbcontext.Contratos
                .Include(c => c.Empleado)
                .Where(c => c.Fecha_Fin >= hoy && c.Fecha_Fin <= limiteContrato)
                .OrderBy(c => c.Fecha_Fin)
                .ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> Reservaciones(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Reservaciones
                .Include(r => r.Cliente)
                .Include(r => r.Mesa_Restaurante)
                .OrderBy(r => r.Fecha_Hora);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        public async Task<IActionResult> Turnos(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Turnos
                .Include(t => t.Empleado_Turnos)
                    .ThenInclude(et => et.Empleado)
                .OrderBy(t => t.ID_Turno);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        public async Task<IActionResult> Pedidos(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var queryBase = _dbcontext.Pedidos
                .Where(p => p.Estado_Pedido != "Completado" && p.Estado_Pedido != "Pagado" && p.Estado_Pedido != "Cancelado");

            ViewBag.TotalPedidos = await queryBase.CountAsync();
            ViewBag.MesasOcupadas = await queryBase.Select(p => p.ID_Mesa).Distinct().CountAsync();
            ViewBag.Pendientes = await queryBase.CountAsync(p => p.Estado_Pedido == "Pendiente");
            ViewBag.Servidos = await queryBase.CountAsync(p => p.Estado_Pedido == "Servido");

            var query = queryBase
                .Include(p => p.Mesa_Restaurante)
                .Include(p => p.Empleado)
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