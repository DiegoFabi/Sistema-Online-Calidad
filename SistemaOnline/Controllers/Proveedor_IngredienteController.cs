using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    // Asociacion Proveedor <-> Ingrediente (M:M). Reutiliza la tabla Proveedores_Ingredientes
    // ya existente en el esquema; no agrega ningun Model ni columna nueva.
    public class Proveedor_IngredienteController : Controller
    {
        private readonly APPDBContext _context;
        public Proveedor_IngredienteController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Proveedores_Ingredientes
                .Include(pi => pi.Proveedor)
                .Include(pi => pi.Ingrediente)
                .OrderBy(pi => pi.Proveedor.Nombre_Empresa).ThenBy(pi => pi.Ingrediente.Nombre_Ingrediente)
                .Select(pi => new Proveedor_IngredienteVM
                {
                    ID_Proveedor = pi.ID_Proveedor,
                    ID_Ingrediente = pi.ID_Ingrediente,
                    ProveedorNombre = pi.Proveedor.Nombre_Empresa,
                    IngredienteNombre = pi.Ingrediente.Nombre_Ingrediente
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
            if (!await _context.Proveedores.AnyAsync() || !await _context.Ingredientes.AnyAsync())
            {
                ViewData["Msg"] = "Debes registrar al menos un Proveedor y un Ingrediente antes de asociarlos.";
                return View("~/Views/Negocio/Advertencia.cshtml");
            }

            var modelo = new Proveedor_IngredienteVM
            {
                ProveedoresDisponibles = await ObtenerProveedores(),
                IngredientesDisponibles = await ObtenerIngredientes()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Proveedor_IngredienteVM modelo)
        {
            if (!await _context.Proveedores.AnyAsync(p => p.ID_Proveedor == modelo.ID_Proveedor))
                ModelState.AddModelError(nameof(modelo.ID_Proveedor), "Selecciona un proveedor válido.");

            if (!await _context.Ingredientes.AnyAsync(i => i.ID_Ingrediente == modelo.ID_Ingrediente))
                ModelState.AddModelError(nameof(modelo.ID_Ingrediente), "Selecciona un ingrediente válido.");

            if (ModelState.IsValid &&
                await _context.Proveedores_Ingredientes.AnyAsync(pi => pi.ID_Proveedor == modelo.ID_Proveedor && pi.ID_Ingrediente == modelo.ID_Ingrediente))
            {
                ModelState.AddModelError(string.Empty, "Ese proveedor ya está asociado a ese ingrediente.");
            }

            if (!ModelState.IsValid)
            {
                modelo.ProveedoresDisponibles = await ObtenerProveedores();
                modelo.IngredientesDisponibles = await ObtenerIngredientes();
                return View(modelo);
            }

            await _context.Proveedores_Ingredientes.AddAsync(new Proveedor_Ingrediente
            {
                ID_Proveedor = modelo.ID_Proveedor,
                ID_Ingrediente = modelo.ID_Ingrediente
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int idProveedor, int idIngrediente)
        {
            var relacion = await _context.Proveedores_Ingredientes
                .FirstOrDefaultAsync(pi => pi.ID_Proveedor == idProveedor && pi.ID_Ingrediente == idIngrediente);
            if (relacion != null)
            {
                _context.Proveedores_Ingredientes.Remove(relacion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerProveedores()
        {
            return await _context.Proveedores.Select(p => new SelectListItem
            {
                Value = p.ID_Proveedor.ToString(),
                Text = p.Nombre_Empresa
            }).ToListAsync();
        }

        private async Task<List<SelectListItem>> ObtenerIngredientes()
        {
            return await _context.Ingredientes.Select(i => new SelectListItem
            {
                Value = i.ID_Ingrediente.ToString(),
                Text = i.Nombre_Ingrediente
            }).ToListAsync();
        }
    }
}