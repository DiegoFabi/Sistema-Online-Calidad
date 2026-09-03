using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class Categoria_IngredienteController : Controller
    {
        private readonly APPDBContext _context;
        public Categoria_IngredienteController(APPDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Categorias_Ingredientes.OrderBy(c => c.ID_Cat_Ingrediente).Select(c => new Categoria_IngredienteVM
            {
                ID_Cat_Ingrediente = c.ID_Cat_Ingrediente,
                Nombre_Categoria = c.Nombre_Categoria
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
            return View(new Categoria_IngredienteVM());
        }
        [HttpPost]
        public async Task<IActionResult> Nuevo(Categoria_IngredienteVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Categoria_Ingrediente categoria_Ingrediente = new Categoria_Ingrediente
            {
                Nombre_Categoria = modelo.Nombre_Categoria
            };
            await _context.Categorias_Ingredientes.AddAsync(categoria_Ingrediente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Categoria_Ingrediente categoria_Ingrediente = await _context.Categorias_Ingredientes.FirstAsync(c => c.ID_Cat_Ingrediente == id);
            Categoria_IngredienteVM modelo = new Categoria_IngredienteVM
            {
                ID_Cat_Ingrediente = categoria_Ingrediente.ID_Cat_Ingrediente,
                Nombre_Categoria = categoria_Ingrediente.Nombre_Categoria
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(Categoria_IngredienteVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Categoria_Ingrediente categoria_Ingrediente = await _context.Categorias_Ingredientes.FirstAsync(c => c.ID_Cat_Ingrediente == modelo.ID_Cat_Ingrediente);
            categoria_Ingrediente.Nombre_Categoria = modelo.Nombre_Categoria;
            _context.Categorias_Ingredientes.Update(categoria_Ingrediente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            Categoria_Ingrediente categoria_Ingrediente = await _context.Categorias_Ingredientes.FirstAsync(c => c.ID_Cat_Ingrediente == id);
            _context.Categorias_Ingredientes.Remove(categoria_Ingrediente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
    }
}