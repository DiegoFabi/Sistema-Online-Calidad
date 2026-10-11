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
                CategoriasDisponibles = await ObtenerCategorias(),
                CategoriasIngredientes = await ObtenerCategoriasIngredientes()
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Nuevo(ProductoVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.CategoriasDisponibles = await ObtenerCategorias();
                modelo.CategoriasIngredientes = await ObtenerCategoriasIngredientes();
                return View(modelo);
            }

            Producto producto = new Producto
            {
                Nombre_Plato = modelo.Nombre_Plato,
                Descripcion = modelo.Descripcion,
                Tiempo_Preparacion = modelo.Tiempo_Preparacion,
                Precio = modelo.Precio,
                Disponibilidad = modelo.Disponibilidad,
                ID_Categoria = modelo.ID_Categoria
            };
            await _context.Productos.AddAsync(producto);
            await _context.SaveChangesAsync();

            await GuardarIngredientesReceta(producto.ID_Producto, modelo.IngredientesSeleccionados, modelo.CantidadesIngredientes);

            Services.NotificacionStore.Agregar("restaurant_menu", "Nuevo ítem en carta", $"Se agregó el producto \"{producto.Nombre_Plato}\".");
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Producto producto = await _context.Productos
                .Include(p => p.Producto_Ingredientes)
                .FirstAsync(p => p.ID_Producto == id);
            ProductoVM modelo = new ProductoVM
            {
                ID_Producto = producto.ID_Producto,
                Nombre_Plato = producto.Nombre_Plato,
                Descripcion = producto.Descripcion,
                Tiempo_Preparacion = producto.Tiempo_Preparacion,
                Precio = producto.Precio,
                Disponibilidad = producto.Disponibilidad,
                ID_Categoria = producto.ID_Categoria,
                CategoriasDisponibles = await ObtenerCategorias(),
                IngredientesSeleccionados = producto.Producto_Ingredientes?.Select(pi => pi.ID_Ingrediente).ToList() ?? new List<int>(),
                CantidadesIngredientes = producto.Producto_Ingredientes?.ToDictionary(pi => pi.ID_Ingrediente, pi => pi.Cantidad) ?? new Dictionary<int, decimal>(),
                CategoriasIngredientes = await ObtenerCategoriasIngredientes()
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(ProductoVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.CategoriasDisponibles = await ObtenerCategorias();
                modelo.CategoriasIngredientes = await ObtenerCategoriasIngredientes();
                return View(modelo);
            }

            Producto producto = await _context.Productos
                .Include(p => p.Producto_Ingredientes)
                .FirstAsync(p => p.ID_Producto == modelo.ID_Producto);
            producto.Nombre_Plato = modelo.Nombre_Plato;
            producto.Descripcion = modelo.Descripcion;
            producto.Tiempo_Preparacion = modelo.Tiempo_Preparacion;
            producto.Precio = modelo.Precio;
            producto.Disponibilidad = modelo.Disponibilidad;
            producto.ID_Categoria = modelo.ID_Categoria;
            _context.Productos.Update(producto);

            // Reemplaza la receta con los ingredientes seleccionados en el modal
            if (producto.Producto_Ingredientes != null && producto.Producto_Ingredientes.Any())
            {
                _context.Productos_Ingredientes.RemoveRange(producto.Producto_Ingredientes);
            }
            await _context.SaveChangesAsync();

            await GuardarIngredientesReceta(producto.ID_Producto, modelo.IngredientesSeleccionados, modelo.CantidadesIngredientes);

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
            if (await _context.Productos_Promociones.AnyAsync(pp => pp.ID_Producto == id))
            {
                TempData["Error"] = "No se puede eliminar un producto que tiene promociones asociadas.";
                return RedirectToAction(nameof(Lista));
            }
            Producto producto = await _context.Productos
                .Include(p => p.Producto_Ingredientes)
                .FirstAsync(p => p.ID_Producto == id);

            // La receta (Producto_Ingrediente) pertenece al producto, no es un registro de negocio
            // independiente como Pedido_Detalle o Producto_Promocion: se borra junto con el producto
            // en vez de bloquear la eliminación.
            if (producto.Producto_Ingredientes != null && producto.Producto_Ingredientes.Any())
            {
                _context.Productos_Ingredientes.RemoveRange(producto.Producto_Ingredientes);
            }

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

        // Ingredientes agrupados por categoría, para el modal "Ingredientes de la Receta"
        private async Task<List<CategoriaIngredientesVM>> ObtenerCategoriasIngredientes()
        {
            var categorias = await _context.Categorias_Ingredientes
                .Include(c => c.Ingredientes)
                .ToListAsync();

            return categorias.Select(c => new CategoriaIngredientesVM
            {
                ID_Cat_Ingrediente = c.ID_Cat_Ingrediente,
                Nombre_Categoria = c.Nombre_Categoria,
                Ingredientes = (c.Ingredientes ?? new List<Ingrediente>())
                    .Select(i => new IngredienteOpcionVM
                    {
                        ID_Ingrediente = i.ID_Ingrediente,
                        Nombre_Ingrediente = i.Nombre_Ingrediente,
                        Unidad_Medida = i.Unidad_Medida
                    }).ToList()
            }).ToList();
        }

        // Crea las filas de Producto_Ingrediente (tabla intermedia) para los ingredientes
        // seleccionados en el modal. Unidad_Medida se toma del propio ingrediente y Observaciones
        // queda vacío: son columnas obligatorias en la tabla intermedia pero no hay campo en el
        // modal para capturarlas, ya que el pedido explícito fue reutilizar la relación existente
        // sin agregar atributos ni modificar modelos.
        private async Task GuardarIngredientesReceta(int idProducto, List<int>? ingredientesSeleccionados, Dictionary<int, decimal>? cantidadesIngredientes)
        {
            if (ingredientesSeleccionados == null || !ingredientesSeleccionados.Any())
            {
                return;
            }

            var ingredientes = await _context.Ingredientes
                .Where(i => ingredientesSeleccionados.Contains(i.ID_Ingrediente))
                .ToListAsync();

            foreach (var ingrediente in ingredientes)
            {
                decimal cantidad = cantidadesIngredientes != null && cantidadesIngredientes.TryGetValue(ingrediente.ID_Ingrediente, out decimal c) && c > 0
                    ? c
                    : 1m;
                await _context.Productos_Ingredientes.AddAsync(new Producto_Ingrediente
                {
                    ID_Producto = idProducto,
                    ID_Ingrediente = ingrediente.ID_Ingrediente,
                    Cantidad = cantidad,
                    Unidad_Medida = ingrediente.Unidad_Medida,
                    Observaciones = string.Empty
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}