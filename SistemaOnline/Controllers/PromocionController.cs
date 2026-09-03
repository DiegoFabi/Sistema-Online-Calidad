using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class PromocionController : Controller
    {
        private readonly APPDBContext _context;
        public PromocionController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Promociones.OrderBy(p => p.ID_Promocion).Select(p => new PromocionVM
            {
                ID_Promocion = p.ID_Promocion,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Porcentaje_Descuento = p.Porcentaje_Descuento,
                Fecha_Inicio = p.Fecha_Inicio,
                Fecha_Fin = p.Fecha_Fin,
                Estado = p.Estado
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
            PromocionVM modelo = new PromocionVM
            {
                Fecha_Inicio = DateTime.Now
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(PromocionVM modelo)
        {
            if (modelo.Fecha_Fin < modelo.Fecha_Inicio)
                ModelState.AddModelError(nameof(modelo.Fecha_Fin), "La fecha de fin debe ser posterior a la fecha de inicio.");

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Promocion promocion = new Promocion
            {
                Nombre = modelo.Nombre,
                Descripcion = modelo.Descripcion,
                Porcentaje_Descuento = modelo.Porcentaje_Descuento,
                Fecha_Inicio = modelo.Fecha_Inicio,
                Fecha_Fin = modelo.Fecha_Fin,
                Estado = modelo.Estado
            };
            await _context.Promociones.AddAsync(promocion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Promocion promocion = await _context.Promociones.FirstAsync(p => p.ID_Promocion == id);
            PromocionVM modelo = new PromocionVM
            {
                ID_Promocion = promocion.ID_Promocion,
                Nombre = promocion.Nombre,
                Descripcion = promocion.Descripcion,
                Porcentaje_Descuento = promocion.Porcentaje_Descuento,
                Fecha_Inicio = promocion.Fecha_Inicio,
                Fecha_Fin = promocion.Fecha_Fin,
                Estado = promocion.Estado
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(PromocionVM modelo)
        {
            if (modelo.Fecha_Fin < modelo.Fecha_Inicio)
                ModelState.AddModelError(nameof(modelo.Fecha_Fin), "La fecha de fin debe ser posterior a la fecha de inicio.");

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Promocion promocion = await _context.Promociones.FirstAsync(p => p.ID_Promocion == modelo.ID_Promocion);
            promocion.Nombre = modelo.Nombre;
            promocion.Descripcion = modelo.Descripcion;
            promocion.Porcentaje_Descuento = modelo.Porcentaje_Descuento;
            promocion.Fecha_Inicio = modelo.Fecha_Inicio;
            promocion.Fecha_Fin = modelo.Fecha_Fin;
            promocion.Estado = modelo.Estado;
            _context.Promociones.Update(promocion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            Promocion promocion = await _context.Promociones.FirstAsync(p => p.ID_Promocion == id);
            _context.Promociones.Remove(promocion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
    }
}