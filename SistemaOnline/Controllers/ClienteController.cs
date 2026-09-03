using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class ClienteController : Controller
    {
        private readonly APPDBContext _context;
        public ClienteController(APPDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Clientes.OrderBy(c => c.ID_Cliente).Select(c => new ClienteVM
            {
                ID_Cliente = c.ID_Cliente,
                Nombre = c.Nombre,
                Apellidos = c.Apellidos,
                Telefono = c.Telefono,
                Email = c.Email,
                Fecha_Nacimiento = c.Fecha_Nacimiento,
                Direccion = c.Direccion,
                DNI = c.DNI,
                RUC = c.RUC
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
            return View(new ClienteVM());
        }
        [HttpPost]
        public async Task<IActionResult> Nuevo(ClienteVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Cliente cliente = new Cliente
            {
                Nombre = modelo.Nombre,
                Apellidos = modelo.Apellidos,
                Telefono = modelo.Telefono,
                Email = modelo.Email,
                Fecha_Nacimiento = modelo.Fecha_Nacimiento,
                Direccion = modelo.Direccion,
                DNI = modelo.DNI,
                RUC = modelo.RUC
            };
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Cliente cliente = await _context.Clientes.FirstAsync(c => c.ID_Cliente == id);
            ClienteVM modelo = new ClienteVM
            {
                ID_Cliente = cliente.ID_Cliente,
                Nombre = cliente.Nombre,
                Apellidos = cliente.Apellidos,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Fecha_Nacimiento = cliente.Fecha_Nacimiento,
                Direccion = cliente.Direccion,
                DNI = cliente.DNI,
                RUC = cliente.RUC
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(ClienteVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Cliente cliente = await _context.Clientes.FirstAsync(c => c.ID_Cliente == modelo.ID_Cliente);
            cliente.Nombre = modelo.Nombre;
            cliente.Apellidos = modelo.Apellidos;
            cliente.Telefono = modelo.Telefono;
            cliente.Email = modelo.Email;
            cliente.Fecha_Nacimiento = modelo.Fecha_Nacimiento;
            cliente.Direccion = modelo.Direccion;
            cliente.DNI = modelo.DNI;
            cliente.RUC = modelo.RUC;
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (await _context.Reservaciones.AnyAsync(r => r.ID_Cliente == id))
            {
                TempData["Error"] = "No se puede eliminar un cliente que tiene reservaciones asociadas.";
                return RedirectToAction(nameof(Lista));
            }
            Cliente cliente = await _context.Clientes.FirstAsync(c => c.ID_Cliente == id);
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
    }
}