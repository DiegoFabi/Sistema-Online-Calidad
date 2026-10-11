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
        public async Task<IActionResult> Nuevo(string? retorno = null)
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
                CategoriasDisponibles = await ObtenerCategorias(),
                Retorno = retorno
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(IngredienteVM modelo)
        {
            modelo.Nombre_Ingrediente = modelo.Nombre_Ingrediente?.Trim();

            if (modelo.Stock_Minimo > modelo.Stock_Maximo)
                ModelState.AddModelError(nameof(modelo.Stock_Minimo), "El stock mínimo no puede ser mayor al stock máximo.");

            if (!string.IsNullOrWhiteSpace(modelo.Nombre_Ingrediente) &&
                await _context.Ingredientes.AnyAsync(i => i.Nombre_Ingrediente == modelo.Nombre_Ingrediente))
                ModelState.AddModelError(nameof(modelo.Nombre_Ingrediente), "Ya existe un ingrediente registrado con ese nombre.");

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

            // El ingrediente se vincula automáticamente a su registro de Inventario con el
            // stock indicado en este mismo formulario: el stock se gestiona en el modelo
            // Inventario (no se agregan columnas de stock a Ingrediente), y así no hace
            // falta crear el inventario por separado — ver también InventarioController,
            // donde se quitó la creación manual de inventarios independientes.
            await _context.Inventarios.AddAsync(new Inventario
            {
                ID_Ingrediente = ingrediente.ID_Ingrediente,
                Cantidad_Stock = modelo.Stock_Actual ?? 0,
                Stock_Minimo = modelo.Stock_Minimo,
                Stock_Maximo = modelo.Stock_Maximo,
                Fecha_Ultima_Reposicion = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Vuelve a Inventario/Lista si se llegó aquí desde ahí (botón "Nuevo Ingrediente"
            // de esa página); si no, al propio listado de Ingredientes.
            if (modelo.Retorno == "Inventario")
            {
                return RedirectToAction("Lista", "Inventario");
            }
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
            modelo.Nombre_Ingrediente = modelo.Nombre_Ingrediente?.Trim();

            if (!string.IsNullOrWhiteSpace(modelo.Nombre_Ingrediente) &&
                await _context.Ingredientes.AnyAsync(i => i.Nombre_Ingrediente == modelo.Nombre_Ingrediente && i.ID_Ingrediente != modelo.ID_Ingrediente))
                ModelState.AddModelError(nameof(modelo.Nombre_Ingrediente), "Ya existe otro ingrediente registrado con ese nombre.");

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
            if (await _context.Productos_Ingredientes.AnyAsync(pi => pi.ID_Ingrediente == id))
            {
                TempData["Error"] = "No se puede eliminar un ingrediente que está en la receta de algún producto.";
                return RedirectToAction(nameof(Lista));
            }
            if (await _context.Proveedores_Ingredientes.AnyAsync(pi => pi.ID_Ingrediente == id))
            {
                TempData["Error"] = "No se puede eliminar un ingrediente que tiene proveedores asociados.";
                return RedirectToAction(nameof(Lista));
            }

            Ingrediente ingrediente = await _context.Ingredientes
                .Include(i => i.Inventarios)
                .FirstAsync(i => i.ID_Ingrediente == id);

            // El Inventario de un ingrediente ya no es un registro independiente: se crea
            // automáticamente junto con el ingrediente (ver Nuevo()), así que se borra junto
            // con él en vez de bloquear la eliminación. Antes bloqueaba siempre que existiera
            // un Inventario asociado, lo cual — tras automatizar su creación — terminaba
            // impidiendo borrar cualquier ingrediente nuevo.
            if (ingrediente.Inventarios != null && ingrediente.Inventarios.Any())
            {
                _context.Inventarios.RemoveRange(ingrediente.Inventarios);
            }

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