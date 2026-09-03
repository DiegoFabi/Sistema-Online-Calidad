using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class ReservacionController : Controller
    {
        private readonly APPDBContext _context;
        public ReservacionController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Reservaciones
                .Include(r => r.Cliente)
                .Include(r => r.Mesa_Restaurante)
                .OrderBy(r => r.ID_Reservacion)
                .Select(r => new ReservacionVM
                {
                    ID_Reservacion = r.ID_Reservacion,
                    Fecha_Hora = r.Fecha_Hora,
                    Numero_Personas = r.Numero_Personas,
                    Ocasion_Especial = r.Ocasion_Especial,
                    Estado_Reservacion = r.Estado_Reservacion,
                    Notas = r.Notas,
                    ID_Cliente = r.ID_Cliente,
                    ID_Mesa = r.ID_Mesa,
                    ClienteNombre = $"{r.Cliente.Nombre} {r.Cliente.Apellidos}",
                    MesaNumero = "Mesa " + r.Mesa_Restaurante.Numero_Mesa
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
            if (!await _context.Clientes.AnyAsync() || !await _context.Mesas.AnyAsync())
            {
                ViewData["Msg"] = "Debes registrar al menos un Cliente y una Mesa antes de crear una Reservación.";
                return View("~/Views/Negocio/Advertencia.cshtml");
            }

            ReservacionVM modelo = new ReservacionVM
            {
                Fecha_Hora = DateTime.Now,
                ClientesDisponibles = await ObtenerClientes(),
                MesasDisponibles = await ObtenerMesas()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(ReservacionVM modelo)
        {
            if (modelo.Fecha_Hora <= DateTime.Now)
                ModelState.AddModelError(nameof(modelo.Fecha_Hora), "La reservación debe ser para una fecha y hora futura.");

            if (!await _context.Clientes.AnyAsync(c => c.ID_Cliente == modelo.ID_Cliente))
                ModelState.AddModelError(nameof(modelo.ID_Cliente), "Selecciona un cliente válido.");
            if (!await _context.Mesas.AnyAsync(m => m.ID_Mesa == modelo.ID_Mesa))
                ModelState.AddModelError(nameof(modelo.ID_Mesa), "Selecciona una mesa válida.");

            if (!ModelState.IsValid)
            {
                modelo.ClientesDisponibles = await ObtenerClientes();
                modelo.MesasDisponibles = await ObtenerMesas();
                return View(modelo);
            }

            Reservacion reservacion = new Reservacion
            {
                Fecha_Hora = modelo.Fecha_Hora,
                Numero_Personas = modelo.Numero_Personas,
                Ocasion_Especial = modelo.Ocasion_Especial,
                Estado_Reservacion = modelo.Estado_Reservacion,
                Notas = modelo.Notas,
                ID_Cliente = modelo.ID_Cliente,
                ID_Mesa = modelo.ID_Mesa
            };
            await _context.Reservaciones.AddAsync(reservacion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Reservacion reservacion = await _context.Reservaciones.FirstAsync(r => r.ID_Reservacion == id);
            ReservacionVM modelo = new ReservacionVM
            {
                ID_Reservacion = reservacion.ID_Reservacion,
                Fecha_Hora = reservacion.Fecha_Hora,
                Numero_Personas = reservacion.Numero_Personas,
                Ocasion_Especial = reservacion.Ocasion_Especial,
                Estado_Reservacion = reservacion.Estado_Reservacion,
                Notas = reservacion.Notas,
                ID_Cliente = reservacion.ID_Cliente,
                ID_Mesa = reservacion.ID_Mesa,
                ClientesDisponibles = await ObtenerClientes(),
                MesasDisponibles = await ObtenerMesas()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(ReservacionVM modelo)
        {
            if (modelo.Fecha_Hora <= DateTime.Now)
                ModelState.AddModelError(nameof(modelo.Fecha_Hora), "La reservación debe ser para una fecha y hora futura.");

            if (!await _context.Clientes.AnyAsync(c => c.ID_Cliente == modelo.ID_Cliente))
                ModelState.AddModelError(nameof(modelo.ID_Cliente), "Selecciona un cliente válido.");
            if (!await _context.Mesas.AnyAsync(m => m.ID_Mesa == modelo.ID_Mesa))
                ModelState.AddModelError(nameof(modelo.ID_Mesa), "Selecciona una mesa válida.");

            if (!ModelState.IsValid)
            {
                modelo.ClientesDisponibles = await ObtenerClientes();
                modelo.MesasDisponibles = await ObtenerMesas();
                return View(modelo);
            }

            Reservacion reservacion = await _context.Reservaciones.FirstAsync(r => r.ID_Reservacion == modelo.ID_Reservacion);
            reservacion.Fecha_Hora = modelo.Fecha_Hora;
            reservacion.Numero_Personas = modelo.Numero_Personas;
            reservacion.Ocasion_Especial = modelo.Ocasion_Especial;
            reservacion.Estado_Reservacion = modelo.Estado_Reservacion;
            reservacion.Notas = modelo.Notas;
            reservacion.ID_Cliente = modelo.ID_Cliente;
            reservacion.ID_Mesa = modelo.ID_Mesa;
            _context.Reservaciones.Update(reservacion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            Reservacion reservacion = await _context.Reservaciones.FirstAsync(r => r.ID_Reservacion == id);
            _context.Reservaciones.Remove(reservacion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerClientes()
        {
            var lista = await _context.Clientes.Select(c => new SelectListItem
            {
                Value = c.ID_Cliente.ToString(),
                Text = c.Nombre + " " + c.Apellidos
            }).ToListAsync();
            return lista;
        }

        private async Task<List<SelectListItem>> ObtenerMesas()
        {
            var lista = await _context.Mesas.Select(m => new SelectListItem
            {
                Value = m.ID_Mesa.ToString(),
                Text = "Mesa " + m.Numero_Mesa
            }).ToListAsync();
            return lista;
        }
    }
}