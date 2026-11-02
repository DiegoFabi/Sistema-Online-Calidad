using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.Services;
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

        // El boton "Reservar" de cada mesa en Kiosco/Reservaciones cae aqui. Exige sesion
        // de Cliente porque la reservacion queda asociada al Cliente autenticado (no hay
        // select de Cliente como en el formulario interno de ReservacionController).
        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public async Task<IActionResult> Reservar(int idMesa)
        {
            var mesa = await _dbcontext.Mesas.FirstOrDefaultAsync(m => m.ID_Mesa == idMesa && m.Estado == "Libre");
            if (mesa == null)
            {
                TempData["Error"] = "Esa mesa ya no está disponible para reservar.";
                return RedirectToAction(nameof(Reservaciones));
            }

            var modelo = new KioscoReservaVM
            {
                ID_Mesa = mesa.ID_Mesa,
                Numero_Mesa = mesa.Numero_Mesa,
                Fecha_Hora = DateTime.Now.AddHours(1),
                Numero_Personas = 2
            };
            return View(modelo);
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<IActionResult> Reservar(KioscoReservaVM modelo)
        {
            var mesa = await ReservaClienteService.RegistrarAsync(_dbcontext, modelo, User, ModelState, "Reservación generada desde el Kiosco.");
            if (mesa == null)
            {
                return View(modelo);
            }

            TempData["Exito"] = $"Tu reservación para la Mesa {mesa.Numero_Mesa} quedó registrada.";
            return RedirectToAction(nameof(Reservaciones));
        }
    }
}