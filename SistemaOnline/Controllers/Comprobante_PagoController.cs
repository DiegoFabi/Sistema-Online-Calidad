using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class Comprobante_PagoController : Controller
    {
        private readonly APPDBContext _context;
        public Comprobante_PagoController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Comprobantes_Pagos.Include(c => c.Pedido).OrderBy(c => c.ID_Comprobante).Select(c => new Comprobante_PagoVM
            {
                ID_Comprobante = c.ID_Comprobante,
                Tipo_Comprobante = c.Tipo_Comprobante,
                Numero_Comprobante = c.Numero_Comprobante,
                Serie = c.Serie,
                Fecha_Emision = c.Fecha_Emision,
                Sub_Total = c.Sub_Total,
                Monto_Total = c.Monto_Total,
                IGV = c.IGV,
                Estado_Comprobante = c.Estado_Comprobante,
                Metodo_Pago = c.Metodo_Pago,
                Razon_Social = c.Razon_Social,
                RUC = c.RUC,
                Direccion_Fiscal = c.Direccion_Fiscal,
                ID_Pedido = c.ID_Pedido,
                PedidoInfo = $"Pedido #{c.Pedido.ID_Pedido} - {c.Pedido.Estado_Pedido}"
            });

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo(int? idPedido)
        {
            if (!await _context.Pedidos.AnyAsync())
            {
                ViewData["Msg"] = "Debes registrar al menos un Pedido antes de registrar Comprobantes de Pago.";
                return View("~/Views/Negocio/Advertencia.cshtml");
            }

            var modelo = new Comprobante_PagoVM
            {
                Fecha_Emision = DateTime.Now,
                ID_Pedido = idPedido ?? 0,
                PedidosDisponibles = await ObtenerPedidos()
            };

            // Pre-poblar totales si viene de un pedido específico
            if (idPedido.HasValue)
            {
                var pedido = await _context.Pedidos.FindAsync(idPedido.Value);
                if (pedido != null)
                {
                    modelo.Sub_Total = pedido.SubTotal;
                    modelo.IGV = Math.Round(pedido.SubTotal * 0.18m, 2);
                    modelo.Monto_Total = pedido.Total;
                }
            }

            ViewBag.ResumenPedidos = await ObtenerResumenPedidos();
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Comprobante_PagoVM modelo)
        {
            // Limpiar validaciones de facturación si no se emite comprobante
            if (!modelo.EmitirComprobante)
            {
                ModelState.Remove(nameof(modelo.Tipo_Comprobante));
                ModelState.Remove(nameof(modelo.Numero_Comprobante));
                ModelState.Remove(nameof(modelo.Serie));
                ModelState.Remove(nameof(modelo.Razon_Social));
                ModelState.Remove(nameof(modelo.RUC));
                ModelState.Remove(nameof(modelo.Direccion_Fiscal));
                modelo.Tipo_Comprobante = null;
                modelo.Numero_Comprobante = null;
                modelo.Serie = null;
                modelo.Razon_Social = null;
                modelo.RUC = null;
                modelo.Direccion_Fiscal = null;
            }
            else
            {
                // Con comprobante: campos base obligatorios
                if (string.IsNullOrWhiteSpace(modelo.Tipo_Comprobante))
                    ModelState.AddModelError(nameof(modelo.Tipo_Comprobante), "El tipo de comprobante es obligatorio.");
                if (string.IsNullOrWhiteSpace(modelo.Numero_Comprobante))
                    ModelState.AddModelError(nameof(modelo.Numero_Comprobante), "El número de comprobante es obligatorio.");
                if (string.IsNullOrWhiteSpace(modelo.Serie))
                    ModelState.AddModelError(nameof(modelo.Serie), "La serie es obligatoria.");
                if (string.IsNullOrWhiteSpace(modelo.Razon_Social))
                    ModelState.AddModelError(nameof(modelo.Razon_Social), "La razón social es obligatoria.");
                if (string.IsNullOrWhiteSpace(modelo.Direccion_Fiscal))
                    ModelState.AddModelError(nameof(modelo.Direccion_Fiscal), "La dirección fiscal es obligatoria.");

                // RUC solo requerido para Factura
                if (modelo.Tipo_Comprobante == "Factura" && string.IsNullOrWhiteSpace(modelo.RUC))
                    ModelState.AddModelError(nameof(modelo.RUC), "El RUC es obligatorio para una Factura.");
                else if (modelo.Tipo_Comprobante != "Factura")
                {
                    ModelState.Remove(nameof(modelo.RUC));
                    modelo.RUC = null;
                }
            }

            if (!ModelState.IsValid)
            {
                modelo.PedidosDisponibles = await ObtenerPedidos();
                ViewBag.ResumenPedidos = await ObtenerResumenPedidos();
                return View(modelo);
            }

            var comprobante = new Comprobante_Pago
            {
                Tipo_Comprobante = modelo.Tipo_Comprobante,
                Numero_Comprobante = modelo.Numero_Comprobante,
                Serie = modelo.Serie,
                Fecha_Emision = modelo.Fecha_Emision,
                Sub_Total = modelo.Sub_Total,
                Monto_Total = modelo.Monto_Total,
                IGV = modelo.IGV,
                Estado_Comprobante = modelo.Estado_Comprobante,
                Metodo_Pago = modelo.Metodo_Pago,
                Razon_Social = modelo.Razon_Social,
                RUC = modelo.RUC,
                Direccion_Fiscal = modelo.Direccion_Fiscal,
                ID_Pedido = modelo.ID_Pedido
            };
            await _context.Comprobantes_Pagos.AddAsync(comprobante);

            // Crear registro en Pagos para que aparezca en el historial de pagos
            var pago = new Pago
            {
                Fecha_Hora_Pago = modelo.Fecha_Emision,
                Monto           = modelo.Monto_Total,
                Metodo_Pago     = modelo.Metodo_Pago,
                Estado          = "Pagado",
                ID_Pedido       = modelo.ID_Pedido
            };
            await _context.Pagos.AddAsync(pago);

            await _context.SaveChangesAsync();

            // Marcar pedido como Pagado y liberar mesa
            var pedido = await _context.Pedidos.Include(p => p.Mesa_Restaurante).FirstOrDefaultAsync(p => p.ID_Pedido == modelo.ID_Pedido);
            if (pedido != null)
            {
                pedido.Estado_Pedido = "Pagado";
                if (pedido.Mesa_Restaurante != null) pedido.Mesa_Restaurante.Estado = "Libre";
                await _context.SaveChangesAsync();
                Services.NotificacionStore.Agregar("receipt_long", "Pago registrado", $"Pedido #{pedido.ID_Pedido} cobrado por {modelo.Metodo_Pago}. Mesa liberada.");
            }

            return RedirectToAction("Index", "Cajero");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Comprobante_Pago comprobante = await _context.Comprobantes_Pagos.FirstAsync(c => c.ID_Comprobante == id);
            Comprobante_PagoVM modelo = new Comprobante_PagoVM
            {
                ID_Comprobante = comprobante.ID_Comprobante,
                Tipo_Comprobante = comprobante.Tipo_Comprobante,
                Numero_Comprobante = comprobante.Numero_Comprobante,
                Serie = comprobante.Serie,
                Fecha_Emision = comprobante.Fecha_Emision,
                Sub_Total = comprobante.Sub_Total,
                Monto_Total = comprobante.Monto_Total,
                IGV = comprobante.IGV,
                Estado_Comprobante = comprobante.Estado_Comprobante,
                Metodo_Pago = comprobante.Metodo_Pago,
                Razon_Social = comprobante.Razon_Social,
                RUC = comprobante.RUC,
                Direccion_Fiscal = comprobante.Direccion_Fiscal,
                ID_Pedido = comprobante.ID_Pedido,
                PedidosDisponibles = await ObtenerPedidos()
            };
            ViewBag.ResumenPedidos = await ObtenerResumenPedidos();
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Comprobante_PagoVM modelo)
        {
            if (!modelo.EmitirComprobante)
            {
                foreach (var field in new[] { nameof(modelo.Tipo_Comprobante), nameof(modelo.Numero_Comprobante), nameof(modelo.Serie), nameof(modelo.Razon_Social), nameof(modelo.RUC), nameof(modelo.Direccion_Fiscal) })
                    ModelState.Remove(field);
                modelo.Tipo_Comprobante = null; modelo.Numero_Comprobante = null; modelo.Serie = null;
                modelo.Razon_Social = null; modelo.RUC = null; modelo.Direccion_Fiscal = null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(modelo.Tipo_Comprobante))
                    ModelState.AddModelError(nameof(modelo.Tipo_Comprobante), "El tipo de comprobante es obligatorio.");
                if (string.IsNullOrWhiteSpace(modelo.Numero_Comprobante))
                    ModelState.AddModelError(nameof(modelo.Numero_Comprobante), "El número de comprobante es obligatorio.");
                if (string.IsNullOrWhiteSpace(modelo.Serie))
                    ModelState.AddModelError(nameof(modelo.Serie), "La serie es obligatoria.");
                if (string.IsNullOrWhiteSpace(modelo.Razon_Social))
                    ModelState.AddModelError(nameof(modelo.Razon_Social), "La razón social es obligatoria.");
                if (string.IsNullOrWhiteSpace(modelo.Direccion_Fiscal))
                    ModelState.AddModelError(nameof(modelo.Direccion_Fiscal), "La dirección fiscal es obligatoria.");

                if (modelo.Tipo_Comprobante == "Factura" && string.IsNullOrWhiteSpace(modelo.RUC))
                    ModelState.AddModelError(nameof(modelo.RUC), "El RUC es obligatorio para una Factura.");
                else if (modelo.Tipo_Comprobante != "Factura") { ModelState.Remove(nameof(modelo.RUC)); modelo.RUC = null; }
            }

            if (!ModelState.IsValid)
            {
                modelo.PedidosDisponibles = await ObtenerPedidos();
                ViewBag.ResumenPedidos = await ObtenerResumenPedidos();
                return View(modelo);
            }

            Comprobante_Pago comprobante = await _context.Comprobantes_Pagos.FirstAsync(c => c.ID_Comprobante == modelo.ID_Comprobante);
            comprobante.Tipo_Comprobante = modelo.Tipo_Comprobante;
            comprobante.Numero_Comprobante = modelo.Numero_Comprobante;
            comprobante.Serie = modelo.Serie;
            comprobante.Fecha_Emision = modelo.Fecha_Emision;
            comprobante.Sub_Total = modelo.Sub_Total;
            comprobante.Monto_Total = modelo.Monto_Total;
            comprobante.IGV = modelo.IGV;
            comprobante.Estado_Comprobante = modelo.Estado_Comprobante;
            comprobante.Metodo_Pago = modelo.Metodo_Pago;
            comprobante.Razon_Social = modelo.Razon_Social;
            comprobante.RUC = modelo.RUC;
            comprobante.Direccion_Fiscal = modelo.Direccion_Fiscal;
            comprobante.ID_Pedido = modelo.ID_Pedido;
            _context.Comprobantes_Pagos.Update(comprobante);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            Comprobante_Pago comprobante = await _context.Comprobantes_Pagos.FirstAsync(c => c.ID_Comprobante == id);
            _context.Comprobantes_Pagos.Remove(comprobante);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerPedidos()
        {
            var pedidosYaCobrados = await _context.Comprobantes_Pagos
                .Select(c => c.ID_Pedido)
                .Distinct()
                .ToListAsync();

            var lista = await _context.Pedidos
                .Where(p => p.Estado_Pedido == "Entregado" && !pedidosYaCobrados.Contains(p.ID_Pedido))
                .Include(p => p.Mesa_Restaurante)
                .Select(p => new SelectListItem
                {
                    Value = p.ID_Pedido.ToString(),
                    Text = "Pedido #" + p.ID_Pedido + " — Mesa " + (p.Mesa_Restaurante != null ? p.Mesa_Restaurante.Numero_Mesa.ToString() : "?")
                }).ToListAsync();
            return lista;
        }

        private async Task<List<ResumenPedidoVM>> ObtenerResumenPedidos()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Include(p => p.Pedido_Detalles)
                    .ThenInclude(pd => pd.Producto)
                .ToListAsync();

            return pedidos.Select(p => new ResumenPedidoVM
            {
                ID_Pedido = p.ID_Pedido,
                MesaNumero = p.Mesa_Restaurante != null ? p.Mesa_Restaurante.Numero_Mesa.ToString() : "-",
                SubTotal = p.SubTotal,
                Total = p.Total,
                Items = p.Pedido_Detalles.Select(pd => new ResumenItemVM
                {
                    Nombre = pd.Producto != null ? pd.Producto.Nombre_Plato : "-",
                    Cantidad = pd.Cantidad,
                    PrecioUnitario = pd.PrecioUnitario
                }).ToList()
            }).ToList();
        }
    }
}