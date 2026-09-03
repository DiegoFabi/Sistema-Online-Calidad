using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class InventarioController : Controller
    {
        private readonly APPDBContext _context;
        public InventarioController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Inventarios.Include(i => i.Ingrediente).OrderBy(i => i.ID_Inventario).Select(i => new InventarioVM
            {
                ID_Inventario = i.ID_Inventario,
                Cantidad_Stock = i.Cantidad_Stock,
                Fecha_Ultima_Reposicion = i.Fecha_Ultima_Reposicion,
                Stock_Minimo = i.Stock_Minimo,
                Stock_Maximo = i.Stock_Maximo,
                ID_Ingrediente = i.ID_Ingrediente,
                IngredienteNombre = i.Ingrediente.Nombre_Ingrediente
            });

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            if (!await _context.Ingredientes.AnyAsync())
            {
                ViewData["Msg"] = "Debes registrar al menos un Ingrediente antes de crear Inventario.";
                return View("~/Views/Negocio/Advertencia.cshtml");
            }

            InventarioVM modelo = new InventarioVM
            {
                Fecha_Ultima_Reposicion = DateTime.Now,
                IngredientesDisponibles = await ObtenerIngredientes()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(InventarioVM modelo)
        {
            if (modelo.Stock_Minimo > modelo.Stock_Maximo)
                ModelState.AddModelError(nameof(modelo.Stock_Minimo), "El stock mínimo no puede ser mayor al stock máximo.");

            if (!ModelState.IsValid)
            {
                modelo.IngredientesDisponibles = await ObtenerIngredientes();
                return View(modelo);
            }

            Inventario inventario = new Inventario
            {
                Cantidad_Stock = modelo.Cantidad_Stock,
                Fecha_Ultima_Reposicion = modelo.Fecha_Ultima_Reposicion,
                Stock_Minimo = modelo.Stock_Minimo,
                Stock_Maximo = modelo.Stock_Maximo,
                ID_Ingrediente = modelo.ID_Ingrediente
            };
            await _context.Inventarios.AddAsync(inventario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Inventario inventario = await _context.Inventarios.FirstAsync(i => i.ID_Inventario == id);
            InventarioVM modelo = new InventarioVM
            {
                ID_Inventario = inventario.ID_Inventario,
                Cantidad_Stock = inventario.Cantidad_Stock,
                Fecha_Ultima_Reposicion = inventario.Fecha_Ultima_Reposicion,
                Stock_Minimo = inventario.Stock_Minimo,
                Stock_Maximo = inventario.Stock_Maximo,
                ID_Ingrediente = inventario.ID_Ingrediente,
                IngredientesDisponibles = await ObtenerIngredientes()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(InventarioVM modelo)
        {
            if (modelo.Stock_Minimo > modelo.Stock_Maximo)
                ModelState.AddModelError(nameof(modelo.Stock_Minimo), "El stock mínimo no puede ser mayor al stock máximo.");

            if (!ModelState.IsValid)
            {
                modelo.IngredientesDisponibles = await ObtenerIngredientes();
                return View(modelo);
            }

            Inventario inventario = await _context.Inventarios.FirstAsync(i => i.ID_Inventario == modelo.ID_Inventario);
            inventario.Cantidad_Stock = modelo.Cantidad_Stock;
            inventario.Fecha_Ultima_Reposicion = modelo.Fecha_Ultima_Reposicion;
            inventario.Stock_Minimo = modelo.Stock_Minimo;
            inventario.Stock_Maximo = modelo.Stock_Maximo;
            inventario.ID_Ingrediente = modelo.ID_Ingrediente;
            _context.Inventarios.Update(inventario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            Inventario inventario = await _context.Inventarios.FirstAsync(i => i.ID_Inventario == id);
            _context.Inventarios.Remove(inventario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerIngredientes()
        {
            var lista = await _context.Ingredientes.Select(i => new SelectListItem
            {
                Value = i.ID_Ingrediente.ToString(),
                Text = i.Nombre_Ingrediente
            }).ToListAsync();
            return lista;
        }
    }
}