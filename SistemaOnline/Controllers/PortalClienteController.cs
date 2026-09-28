using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
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
    }
}