using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Controllers
{
    public class KioscoController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public KioscoController(APPDBContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public async Task<IActionResult> Index(int? categoria)
        {
            var query = _dbcontext.Productos
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

            var modelo = new KioscoMenuVM
            {
                CategoriaSeleccionada = categoria,
                Categorias = await _dbcontext.Productos_Categorias.OrderBy(c => c.Nombre_Categoria).ToListAsync(),
                Productos = productos
            };
            return View(modelo);
        }

        public async Task<IActionResult> Reservaciones()
        {
            var mesas = await _dbcontext.Mesas.Where(m => m.Estado == "Libre").OrderBy(m => m.Numero_Mesa).ToListAsync();
            return View(mesas);
        }
    }
}