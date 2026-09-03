using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class PagoController : Controller
    {
        private readonly APPDBContext _context;
        public PagoController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Pagos.Include(p => p.Pedido).OrderBy(p => p.ID_Pago).Select(p => new PagoVM
            {
                ID_Pago = p.ID_Pago,
                Fecha_Hora_Pago = p.Fecha_Hora_Pago,
                Monto = p.Monto,
                Metodo_Pago = p.Metodo_Pago,
                Detalles_Tarjeta = p.Detalles_Tarjeta,
                Estado = p.Estado,
                ID_Pedido = p.ID_Pedido,
                PedidoInfo = $"Pedido #{p.Pedido.ID_Pedido} - {p.Pedido.Estado_Pedido}"
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
            if (!await _context.Pedidos.AnyAsync())
            {
                ViewData["Msg"] = "Debes registrar al menos un Pedido antes de registrar Pagos.";
                return View("~/Views/Negocio/Advertencia.cshtml");
            }

            PagoVM modelo = new PagoVM
            {
                Fecha_Hora_Pago = DateTime.Now,
                PedidosDisponibles = await ObtenerPedidos()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(PagoVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.PedidosDisponibles = await ObtenerPedidos();
                return View(modelo);
            }

            Pago pago = new Pago
            {
                Fecha_Hora_Pago = modelo.Fecha_Hora_Pago,
                Monto = modelo.Monto,
                Metodo_Pago = modelo.Metodo_Pago,
                Detalles_Tarjeta = modelo.Detalles_Tarjeta,
                Estado = modelo.Estado,
                ID_Pedido = modelo.ID_Pedido
            };
            await _context.Pagos.AddAsync(pago);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Pago pago = await _context.Pagos.FirstAsync(p => p.ID_Pago == id);
            PagoVM modelo = new PagoVM
            {
                ID_Pago = pago.ID_Pago,
                Fecha_Hora_Pago = pago.Fecha_Hora_Pago,
                Monto = pago.Monto,
                Metodo_Pago = pago.Metodo_Pago,
                Detalles_Tarjeta = pago.Detalles_Tarjeta,
                Estado = pago.Estado,
                ID_Pedido = pago.ID_Pedido,
                PedidosDisponibles = await ObtenerPedidos()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(PagoVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.PedidosDisponibles = await ObtenerPedidos();
                return View(modelo);
            }

            Pago pago = await _context.Pagos.FirstAsync(p => p.ID_Pago == modelo.ID_Pago);
            pago.Fecha_Hora_Pago = modelo.Fecha_Hora_Pago;
            pago.Monto = modelo.Monto;
            pago.Metodo_Pago = modelo.Metodo_Pago;
            pago.Detalles_Tarjeta = modelo.Detalles_Tarjeta;
            pago.Estado = modelo.Estado;
            pago.ID_Pedido = modelo.ID_Pedido;
            _context.Pagos.Update(pago);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            var pago = await _context.Pagos.Include(p => p.Pedido).FirstAsync(p => p.ID_Pago == id);
            if (pago.Pedido?.Estado_Pedido == "Pagado")
            {
                TempData["Error"] = "No se puede eliminar un pago cuyo pedido ya está en estado Pagado.";
                return RedirectToAction(nameof(Lista));
            }
            _context.Pagos.Remove(pago);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerPedidos()
        {
            var estados = new[] { "Pendiente", "En Cocina", "Preparando", "Listo", "Entregado" };
            var lista = await _context.Pedidos
                .Where(p => estados.Contains(p.Estado_Pedido))
                .Select(p => new SelectListItem
                {
                    Value = p.ID_Pedido.ToString(),
                    Text = "Pedido #" + p.ID_Pedido + " - " + p.Estado_Pedido
                }).ToListAsync();
            return lista;
        }
    }
}