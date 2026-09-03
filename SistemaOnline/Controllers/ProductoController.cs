using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class ProductoController : Controller
    {
        private readonly APPDBContext _context;
        public ProductoController(APPDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = SistemaOnline.Services.PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Productos.Include(p => p.Producto_Categoria).OrderBy(p => p.ID_Producto).Select(p => new ProductoVM
            {
                ID_Producto = p.ID_Producto,
                Nombre_Plato = p.Nombre_Plato,
                Descripcion = p.Descripcion,
                Tiempo_Preparacion = p.Tiempo_Preparacion,
                Precio = p.Precio,
                Disponibilidad = p.Disponibilidad,
                Categoria = p.Categoria,
                ID_Categoria = p.ID_Categoria,
                CategoriaRelNombre = p.Producto_Categoria.Nombre_Categoria
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
            if (!await _context.Productos_Categorias.AnyAsync())
            {
                ViewData["Msg"] = "Debes crear al menos una Categoría de Producto antes de registrar Productos.";
                return View("~/Views/Negocio/Advertencia.cshtml");
            }

            ProductoVM modelo = new ProductoVM
            {
                CategoriasDisponibles = await ObtenerCategorias()
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Nuevo(ProductoVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.CategoriasDisponibles = await ObtenerCategorias();
                return View(modelo);
            }

            Producto producto = new Producto
            {
                Nombre_Plato = modelo.Nombre_Plato,
                Descripcion = modelo.Descripcion,
                Tiempo_Preparacion = modelo.Tiempo_Preparacion,
                Precio = modelo.Precio,
                Disponibilidad = modelo.Disponibilidad,
                Categoria = modelo.Categoria,
                ID_Categoria = modelo.ID_Categoria
            };
            await _context.Productos.AddAsync(producto);
            await _context.SaveChangesAsync();
            Services.NotificacionStore.Agregar("restaurant_menu", "Nuevo ítem en carta", $"Se agregó el producto \"{producto.Nombre_Plato}\".");
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Producto producto = await _context.Productos.FirstAsync(p => p.ID_Producto == id);
            ProductoVM modelo = new ProductoVM
            {
                ID_Producto = producto.ID_Producto,
                Nombre_Plato = producto.Nombre_Plato,
                Descripcion = producto.Descripcion,
                Tiempo_Preparacion = producto.Tiempo_Preparacion,
                Precio = producto.Precio,
                Disponibilidad = producto.Disponibilidad,
                Categoria = producto.Categoria,
                ID_Categoria = producto.ID_Categoria,
                CategoriasDisponibles = await ObtenerCategorias()
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(ProductoVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.CategoriasDisponibles = await ObtenerCategorias();
                return View(modelo);
            }

            Producto producto = await _context.Productos.FirstAsync(p => p.ID_Producto == modelo.ID_Producto);
            producto.Nombre_Plato = modelo.Nombre_Plato;
            producto.Descripcion = modelo.Descripcion;
            producto.Tiempo_Preparacion = modelo.Tiempo_Preparacion;
            producto.Precio = modelo.Precio;
            producto.Disponibilidad = modelo.Disponibilidad;
            producto.Categoria = modelo.Categoria;
            producto.ID_Categoria = modelo.ID_Categoria;
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (await _context.Pedidos_Detalles.AnyAsync(pd => pd.ID_Producto == id))
            {
                TempData["Error"] = "No se puede eliminar un producto que tiene detalles de pedidos asociados.";
                return RedirectToAction(nameof(Lista));
            }
            Producto producto = await _context.Productos.FirstAsync(p => p.ID_Producto == id);
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerCategorias()
        {
            var lista = await _context.Productos_Categorias.Select(c => new SelectListItem
            {
                Value = c.ID_Categoria.ToString(),
                Text = c.Nombre_Categoria
            }).ToListAsync();
            return lista;
        }
    }
}