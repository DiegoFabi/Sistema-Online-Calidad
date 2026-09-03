using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;

namespace SistemaOnline.Controllers
{
    public class KioscoController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public KioscoController(APPDBContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _dbcontext.Productos
                .Include(p => p.Producto_Categoria)
                .Where(p => p.Disponibilidad)
                .OrderBy(p => p.Producto_Categoria.Nombre_Categoria)
                .ToListAsync();
            return View(productos);
        }

        public async Task<IActionResult> Reservaciones()
        {
            var mesas = await _dbcontext.Mesas.Where(m => m.Estado == "Libre").OrderBy(m => m.Numero_Mesa).ToListAsync();
            return View(mesas);
        }
    }
}