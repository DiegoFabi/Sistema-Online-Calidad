using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.Services;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Controllers
{
    // Portal exclusivo para el rol Cliente (autoservicio): consulta de carta y
    // promociones. Todo el contenido se lee directamente de APPDBContext, sin
    // tablas ni entidades nuevas.
    [Authorize(Roles = "Cliente")]
    public class PortalClienteController : Controller
    {
        private readonly APPDBContext _context;
        public PortalClienteController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? categoria)
        {
            var categorias = await _context.Productos_Categorias
                .OrderBy(c => c.Nombre_Categoria)
                .ToListAsync();

            var query = _context.Productos
                .Include(p => p.Producto_Categoria)
                .Where(p => p.Disponibilidad);

            if (categoria.HasValue)
            {
                query = query.Where(p => p.ID_Categoria == categoria.Value);
            }

            var productos = await query
                .OrderBy(p => p.Producto_Categoria.Nombre_Categoria)
                .ThenBy(p => p.Nombre_Plato)
                .ToListAsync();

            var ahora = DateTime.Now;
            var promocionesActivas = await _context.Promociones
                .Where(pr => pr.Estado && pr.Fecha_Inicio <= ahora && pr.Fecha_Fin >= ahora)
                .OrderBy(pr => pr.Fecha_Fin)
                .ToListAsync();

            var modelo = new PortalClienteMenuVM
            {
                Categorias = categorias,
                CategoriaSeleccionada = categoria,
                Productos = productos,
                PromocionesActivas = promocionesActivas
            };
            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Promociones()
        {
            var promociones = await _context.Promociones
                .Where(pr => pr.Estado)
                .OrderByDescending(pr => pr.Fecha_Inicio)
                .ToListAsync();
            return View(promociones);
        }

        [HttpGet]
        public async Task<IActionResult> Reservaciones()
        {
            var mesas = await _context.Mesas.Where(m => m.Estado == "Libre").OrderBy(m => m.Numero_Mesa).ToListAsync();
            return View(mesas);
        }

        // Mismo flujo que KioscoController.Reservar (el Cliente no elige de un select,
        // se resuelve del Usuario autenticado); se repite aqui porque esta es la pantalla
        // a la que en verdad llega un Cliente ya logueado (Kiosco es solo para el visitante
        // anonimo, que aqui ya tiene su propio punto de entrada con sidebar).
        [HttpGet]
        public async Task<IActionResult> Reservar(int idMesa)
        {
            var mesa = await _context.Mesas.FirstOrDefaultAsync(m => m.ID_Mesa == idMesa && m.Estado == "Libre");
            if (mesa == null)
            {
                TempData["Error"] = "Esa mesa ya no está disponible para reservar.";
                return RedirectToAction(nameof(Reservaciones));
            }

            var modelo = new KioscoReservaVM
            {
                ID_Mesa = mesa.ID_Mesa,
                Numero_Mesa = mesa.Numero_Mesa,
                Fecha_Hora = DateTime.Now.AddHours(1),
                Numero_Personas = 2
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Reservar(KioscoReservaVM modelo)
        {
            var mesa = await ReservaClienteService.RegistrarAsync(_context, modelo, User, ModelState, "Reservación generada desde el Portal del Cliente.");
            if (mesa == null)
            {
                return View(modelo);
            }

            TempData["Exito"] = $"Tu reservación para la Mesa {mesa.Numero_Mesa} quedó registrada.";
            return RedirectToAction(nameof(Reservaciones));
        }
    }
}
