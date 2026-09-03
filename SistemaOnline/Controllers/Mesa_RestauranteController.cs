using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class Mesa_RestauranteController : Controller
    {
        private readonly APPDBContext _context;
        public Mesa_RestauranteController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Mesas.OrderBy(m => m.ID_Mesa).Select(m => new Mesa_RestauranteVM
            {
                ID_Mesa = m.ID_Mesa,
                Numero_Mesa = m.Numero_Mesa,
                Capacidad = m.Capacidad,
                Ubicacion = m.Ubicacion,
                Estado = m.Estado
            });

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            return View(new Mesa_RestauranteVM());
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Mesa_RestauranteVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Mesa_Restaurante mesa = new Mesa_Restaurante
            {
                Numero_Mesa = modelo.Numero_Mesa,
                Capacidad = modelo.Capacidad,
                Ubicacion = modelo.Ubicacion,
                Estado = modelo.Estado
            };
            await _context.Mesas.AddAsync(mesa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Mesa_Restaurante mesa = await _context.Mesas.FirstAsync(m => m.ID_Mesa == id);
            Mesa_RestauranteVM modelo = new Mesa_RestauranteVM
            {
                ID_Mesa = mesa.ID_Mesa,
                Numero_Mesa = mesa.Numero_Mesa,
                Capacidad = mesa.Capacidad,
                Ubicacion = mesa.Ubicacion,
                Estado = mesa.Estado
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Mesa_RestauranteVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Mesa_Restaurante mesa = await _context.Mesas.FirstAsync(m => m.ID_Mesa == modelo.ID_Mesa);
            var estadosActivos = new[] { "Pendiente", "En Cocina", "Preparando", "Listo", "Entregado" };
            if (await _context.Pedidos.AnyAsync(p => p.ID_Mesa == modelo.ID_Mesa && estadosActivos.Contains(p.Estado_Pedido)))
            {
                ModelState.AddModelError(string.Empty, "No se puede editar una mesa con pedidos activos.");
                return View(modelo);
            }
            mesa.Numero_Mesa = modelo.Numero_Mesa;
            mesa.Capacidad = modelo.Capacidad;
            mesa.Ubicacion = modelo.Ubicacion;
            mesa.Estado = modelo.Estado;
            _context.Mesas.Update(mesa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (await _context.Pedidos.AnyAsync(p => p.ID_Mesa == id))
            {
                TempData["Error"] = "No se puede eliminar una mesa que tiene pedidos asociados.";
                return RedirectToAction(nameof(Lista));
            }
            Mesa_Restaurante mesa = await _context.Mesas.FirstAsync(m => m.ID_Mesa == id);
            _context.Mesas.Remove(mesa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
    }
}