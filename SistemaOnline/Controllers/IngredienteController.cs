using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class IngredienteController : Controller
    {
        private readonly APPDBContext _context;
        public IngredienteController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Ingredientes.Include(i => i.Categoria_Ingrediente).OrderBy(i => i.ID_Ingrediente).Select(i => new IngredienteVM
            {
                ID_Ingrediente = i.ID_Ingrediente,
                Nombre_Ingrediente = i.Nombre_Ingrediente,
                Unidad_Medida = i.Unidad_Medida,
                Descripcion = i.Descripcion,
                Costo_Unitario = i.Costo_Unitario,
                Estado = i.Estado,
                ID_Cat_Ingrediente = i.ID_Cat_Ingrediente,
                CategoriaNombre = i.Categoria_Ingrediente.Nombre_Categoria
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
            if (!await _context.Categorias_Ingredientes.AnyAsync())
            {
                ViewData["Msg"] = "Debes crear al menos una Categoría de Ingrediente antes de registrar Ingredientes.";
                return View("~/Views/Negocio/Advertencia.cshtml");
            }

            IngredienteVM modelo = new IngredienteVM
            {
                Descripcion = "¡Nada por agregar!",
                Estado = true,
                CategoriasDisponibles = await ObtenerCategorias()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(IngredienteVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.CategoriasDisponibles = await ObtenerCategorias();
                return View(modelo);
            }

            Ingrediente ingrediente = new Ingrediente
            {
                Nombre_Ingrediente = modelo.Nombre_Ingrediente,
                Unidad_Medida = modelo.Unidad_Medida,
                Descripcion = modelo.Descripcion,
                Costo_Unitario = modelo.Costo_Unitario,
                Estado = modelo.Estado,
                ID_Cat_Ingrediente = modelo.ID_Cat_Ingrediente
            };
            await _context.Ingredientes.AddAsync(ingrediente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Ingrediente ingrediente = await _context.Ingredientes.FirstAsync(i => i.ID_Ingrediente == id);
            IngredienteVM modelo = new IngredienteVM
            {
                ID_Ingrediente = ingrediente.ID_Ingrediente,
                Nombre_Ingrediente = ingrediente.Nombre_Ingrediente,
                Unidad_Medida = ingrediente.Unidad_Medida,
                Descripcion = ingrediente.Descripcion,
                Costo_Unitario = ingrediente.Costo_Unitario,
                Estado = ingrediente.Estado,
                ID_Cat_Ingrediente = ingrediente.ID_Cat_Ingrediente,
                CategoriasDisponibles = await ObtenerCategorias()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(IngredienteVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.CategoriasDisponibles = await ObtenerCategorias();
                return View(modelo);
            }

            Ingrediente ingrediente = await _context.Ingredientes.FirstAsync(i => i.ID_Ingrediente == modelo.ID_Ingrediente);
            ingrediente.Nombre_Ingrediente = modelo.Nombre_Ingrediente;
            ingrediente.Unidad_Medida = modelo.Unidad_Medida;
            ingrediente.Descripcion = modelo.Descripcion;
            ingrediente.Costo_Unitario = modelo.Costo_Unitario;
            ingrediente.Estado = modelo.Estado;
            ingrediente.ID_Cat_Ingrediente = modelo.ID_Cat_Ingrediente;
            _context.Ingredientes.Update(ingrediente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (await _context.Inventarios.AnyAsync(i => i.ID_Ingrediente == id))
            {
                TempData["Error"] = "No se puede eliminar un ingrediente que tiene registros de inventario asociados.";
                return RedirectToAction(nameof(Lista));
            }
            Ingrediente ingrediente = await _context.Ingredientes.FirstAsync(i => i.ID_Ingrediente == id);
            _context.Ingredientes.Remove(ingrediente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerCategorias()
        {
            var lista = await _context.Categorias_Ingredientes.Select(c => new SelectListItem
            {
                Value = c.ID_Cat_Ingrediente.ToString(),
                Text = c.Nombre_Categoria
            }).ToListAsync();
            return lista;
        }
    }
}