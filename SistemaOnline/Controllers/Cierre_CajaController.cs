using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Services;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Controllers
{
    // Cierre de Caja: reporte calculado en vivo a partir de Pagos ya existentes.
    // No introduce tablas ni entidades nuevas; solo agrupa y suma datos que el
    // sistema ya registra al cobrar un pedido (Comprobante_PagoController).
    [Authorize(Roles = "Administrador,Cajero")]
    public class Cierre_CajaController : Controller
    {
        private readonly APPDBContext _context;
        public Cierre_CajaController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fecha)
        {
            var dia = (fecha ?? DateTime.Today).Date;
            var siguienteDia = dia.AddDays(1);

            var pagosDelDia = await _context.Pagos
                .Include(pg => pg.Pedido)
                .Where(pg => pg.Fecha_Hora_Pago >= dia && pg.Fecha_Hora_Pago < siguienteDia && pg.Estado == "Pagado")
                .OrderByDescending(pg => pg.Fecha_Hora_Pago)
                .ToListAsync();

            var modelo = new Cierre_CajaVM
            {
                Fecha = dia,
                TotalGeneral = pagosDelDia.Sum(pg => pg.Monto),
                CantidadPagos = pagosDelDia.Count,
                Pagos = pagosDelDia,
                MetodosPago = pagosDelDia
                    .GroupBy(pg => pg.Metodo_Pago)
                    .Select(g => new MetodoPagoVM
                    {
                        Metodo = g.Key,
                        Total = g.Sum(pg => pg.Monto),
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(m => m.Total)
                    .ToList(),
                EfectivoEsperado = pagosDelDia.Where(pg => pg.Metodo_Pago == "Efectivo").Sum(pg => pg.Monto),
                CierreRegistrado = CierreCajaStore.ObtenerPorFecha(dia)
            };

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCierre(DateTime fecha, decimal efectivoContado)
        {
            var dia = fecha.Date;

            if (CierreCajaStore.ObtenerPorFecha(dia) != null)
            {
                TempData["Error"] = "La caja de ese día ya fue cerrada.";
                return RedirectToAction(nameof(Index), new { fecha = dia });
            }

            var siguienteDia = dia.AddDays(1);
            var pagosDelDia = await _context.Pagos
                .Where(pg => pg.Fecha_Hora_Pago >= dia && pg.Fecha_Hora_Pago < siguienteDia && pg.Estado == "Pagado")
                .ToListAsync();

            decimal efectivoEsperado = pagosDelDia.Where(pg => pg.Metodo_Pago == "Efectivo").Sum(pg => pg.Monto);
            decimal totalGeneral = pagosDelDia.Sum(pg => pg.Monto);

            CierreCajaStore.Registrar(User, dia, efectivoEsperado, efectivoContado, totalGeneral);

            TempData["Exito"] = "Cierre de caja registrado correctamente.";
            return RedirectToAction(nameof(Index), new { fecha = dia });
        }
    }
}