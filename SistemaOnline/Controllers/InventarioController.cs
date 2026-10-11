using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class InventarioController : Controller
    {
        private readonly APPDBContext _context;
        private readonly IEmailService _email;
        public InventarioController(APPDBContext context, IEmailService email)
        {
            _context = context;
            _email = email;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Inventarios.Include(i => i.Ingrediente).OrderBy(i => i.ID_Inventario).Select(i => new InventarioVM
            {
                ID_Inventario = i.ID_Inventario,
                Cantidad_Stock = i.Cantidad_Stock,
                Fecha_Ultima_Reposicion = i.Fecha_Ultima_Reposicion,
                Stock_Minimo = i.Stock_Minimo,
                Stock_Maximo = i.Stock_Maximo,
                ID_Ingrediente = i.ID_Ingrediente,
                IngredienteNombre = i.Ingrediente.Nombre_Ingrediente
            });

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        // Ya no se crean registros de Inventario de forma manual/independiente: se generan
        // automáticamente al registrar un Ingrediente (ver IngredienteController.Nuevo), para
        // evitar listas de inventario desvinculadas de un ingrediente real. Cualquier enlace
        // o marcador antiguo a "Nuevo Inventario" se redirige al flujo correcto.
        [HttpGet]
        public IActionResult Nuevo()
        {
            return RedirectToAction("Nuevo", "Ingrediente");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Inventario inventario = await _context.Inventarios.FirstAsync(i => i.ID_Inventario == id);
            InventarioVM modelo = new InventarioVM
            {
                ID_Inventario = inventario.ID_Inventario,
                Cantidad_Stock = inventario.Cantidad_Stock,
                Fecha_Ultima_Reposicion = inventario.Fecha_Ultima_Reposicion,
                Stock_Minimo = inventario.Stock_Minimo,
                Stock_Maximo = inventario.Stock_Maximo,
                ID_Ingrediente = inventario.ID_Ingrediente,
                IngredientesDisponibles = await ObtenerIngredientes()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(InventarioVM modelo)
        {
            if (modelo.Stock_Minimo > modelo.Stock_Maximo)
                ModelState.AddModelError(nameof(modelo.Stock_Minimo), "El stock mínimo no puede ser mayor al stock máximo.");

            if (!ModelState.IsValid)
            {
                modelo.IngredientesDisponibles = await ObtenerIngredientes();
                return View(modelo);
            }

            Inventario inventario = await _context.Inventarios.FirstAsync(i => i.ID_Inventario == modelo.ID_Inventario);
            inventario.Cantidad_Stock = modelo.Cantidad_Stock;
            inventario.Fecha_Ultima_Reposicion = modelo.Fecha_Ultima_Reposicion;
            inventario.Stock_Minimo = modelo.Stock_Minimo;
            inventario.Stock_Maximo = modelo.Stock_Maximo;
            inventario.ID_Ingrediente = modelo.ID_Ingrediente;
            _context.Inventarios.Update(inventario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            Inventario inventario = await _context.Inventarios.FirstAsync(i => i.ID_Inventario == id);
            _context.Inventarios.Remove(inventario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        // Envía una solicitud de compra a un proveedor (no es un pedido/orden con su propio
        // seguimiento: repone el stock al momento, igual que antes, pero el lenguaje de cara
        // al usuario refleja que es una solicitud que se le manda al proveedor, no una compra
        // ya cerrada). No crea ninguna tabla nueva: reutiliza Inventario.Cantidad_Stock, ya
        // existente, y deja constancia en el historial de Auditoria, en memoria.
        [HttpGet]
        public async Task<IActionResult> RegistrarCompra()
        {
            if (!await _context.Proveedores.AnyAsync() || !await _context.Inventarios.AnyAsync())
            {
                ViewData["Msg"] = "Debes registrar al menos un Proveedor y un Inventario antes de enviar una solicitud de compra.";
                return View("~/Views/Negocio/Advertencia.cshtml");
            }

            var modelo = new RegistrarCompraVM
            {
                ProveedoresDisponibles = await ObtenerProveedores(),
                IngredientesParaComprar = await ObtenerIngredientesParaComprar()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCompra(RegistrarCompraVM modelo)
        {
            var proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.ID_Proveedor == modelo.ID_Proveedor);
            if (proveedor == null)
                ModelState.AddModelError(nameof(modelo.ID_Proveedor), "Selecciona un proveedor válido.");

            if (modelo.IngredientesSeleccionados == null || !modelo.IngredientesSeleccionados.Any())
                ModelState.AddModelError(string.Empty, "Selecciona al menos un ingrediente para solicitar.");

            List<Inventario> inventariosAActualizar = new();
            if (proveedor != null && modelo.IngredientesSeleccionados != null && modelo.IngredientesSeleccionados.Any())
            {
                // El ingrediente debe estar ligado al proveedor elegido: no se confía en el
                // filtro del modal (cliente), se revalida aquí contra Proveedores_Ingredientes.
                var idsLigados = await _context.Proveedores_Ingredientes
                    .Where(pi => pi.ID_Proveedor == modelo.ID_Proveedor && modelo.IngredientesSeleccionados.Contains(pi.ID_Ingrediente))
                    .Select(pi => pi.ID_Ingrediente)
                    .ToListAsync();
                if (modelo.IngredientesSeleccionados.Except(idsLigados).Any())
                    ModelState.AddModelError(string.Empty, "Uno o más ingredientes seleccionados no están asociados a ese proveedor.");

                foreach (var idIngrediente in modelo.IngredientesSeleccionados)
                {
                    bool tieneCantidad = modelo.CantidadesIngredientes != null
                        && modelo.CantidadesIngredientes.TryGetValue(idIngrediente, out decimal cantidad)
                        && cantidad > 0;
                    if (!tieneCantidad)
                        ModelState.AddModelError(string.Empty, "La cantidad a solicitar de cada ingrediente debe ser mayor a 0.");
                }

                inventariosAActualizar = await _context.Inventarios
                    .Include(i => i.Ingrediente)
                    .Where(i => modelo.IngredientesSeleccionados.Contains(i.ID_Ingrediente))
                    .ToListAsync();
                if (inventariosAActualizar.Count != modelo.IngredientesSeleccionados.Count)
                    ModelState.AddModelError(string.Empty, "Algún ingrediente seleccionado no tiene un registro de inventario asociado.");
            }

            if (!ModelState.IsValid)
            {
                modelo.ProveedoresDisponibles = await ObtenerProveedores();
                modelo.IngredientesParaComprar = await ObtenerIngredientesParaComprar();
                return View(modelo);
            }

            var detalle = new List<string>();
            decimal totalEstimado = 0;
            var filasCorreo = new List<string>();
            foreach (var inventario in inventariosAActualizar)
            {
                decimal cantidad = modelo.CantidadesIngredientes![inventario.ID_Ingrediente];
                decimal stockAnterior = inventario.Cantidad_Stock;
                inventario.Cantidad_Stock += cantidad;
                inventario.Fecha_Ultima_Reposicion = DateTime.Now;
                detalle.Add($"{inventario.Ingrediente.Nombre_Ingrediente} (+{cantidad:0.###} {inventario.Ingrediente.Unidad_Medida}, {stockAnterior:0.###} → {inventario.Cantidad_Stock:0.###})");

                decimal subtotal = cantidad * inventario.Ingrediente.Costo_Unitario;
                totalEstimado += subtotal;
                filasCorreo.Add($"<tr><td>{inventario.Ingrediente.Nombre_Ingrediente}</td><td style=\"text-align:right\">{cantidad:0.###} {inventario.Ingrediente.Unidad_Medida}</td>" +
                    $"<td style=\"text-align:right\">S/ {inventario.Ingrediente.Costo_Unitario:0.00}</td><td style=\"text-align:right\">S/ {subtotal:0.00}</td></tr>");
            }
            await _context.SaveChangesAsync();

            string resumen = string.Join("; ", detalle);
            AuditoriaStore.Registrar(User, "Solicitud de compra a proveedor", "Inventario",
                $"Se envió una solicitud de compra a '{proveedor!.Nombre_Empresa}': {resumen}.");

            NotificacionStore.Agregar("inventory_2", "Solicitud de compra enviada",
                $"Se solicitó reposición de stock a '{proveedor.Nombre_Empresa}': {resumen}.");

            // Correo real al proveedor (no simulado): si el SMTP no está configurado o falla,
            // el stock ya quedó actualizado (arriba), pero se lo decimos explícitamente al
            // usuario en vez de aparentar que el correo salió cuando no fue así.
            string asunto = $"Solicitud de compra — El Rancho Sagrado (#{DateTime.Now:yyyyMMddHHmmss})";
            string cuerpoHtml = $@"
                <p>Estimado(a) {proveedor.Nombre_Empresa},</p>
                <p>El Rancho Sagrado solicita la reposición de los siguientes ingredientes:</p>
                <table style=""border-collapse:collapse;width:100%"" cellpadding=""6"">
                    <thead>
                        <tr style=""background:#f3f4f5;text-align:left"">
                            <th>Ingrediente</th><th style=""text-align:right"">Cantidad</th>
                            <th style=""text-align:right"">Costo Unitario</th><th style=""text-align:right"">Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>{string.Join("", filasCorreo)}</tbody>
                </table>
                <p><strong>Total estimado: S/ {totalEstimado:0.00}</strong></p>
                <p>Por favor confirma la entrega respondiendo a este correo.</p>
                <p>— El Rancho Sagrado</p>";

            try
            {
                await _email.EnviarAsync(proveedor.Email_Contacto, asunto, cuerpoHtml);
                TempData["Exito"] = $"Solicitud de compra enviada por correo a '{proveedor.Nombre_Empresa}' ({proveedor.Email_Contacto}). Se repuso el stock de {inventariosAActualizar.Count} ingrediente(s).";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"El stock se actualizó, pero no se pudo enviar el correo a '{proveedor.Nombre_Empresa}' ({proveedor.Email_Contacto}): {ex.Message}";
            }

            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerIngredientes()
        {
            var lista = await _context.Ingredientes.Select(i => new SelectListItem
            {
                Value = i.ID_Ingrediente.ToString(),
                Text = i.Nombre_Ingrediente
            }).ToListAsync();
            return lista;
        }

        private async Task<List<SelectListItem>> ObtenerProveedores()
        {
            return await _context.Proveedores.Select(p => new SelectListItem
            {
                Value = p.ID_Proveedor.ToString(),
                Text = p.Nombre_Empresa
            }).ToListAsync();
        }

        // Ingredientes que se pueden solicitar (los que ya tienen Inventario), con su stock
        // actual y los proveedores que los suministran, para el modal de RegistrarCompra.
        private async Task<List<IngredienteCompraVM>> ObtenerIngredientesParaComprar()
        {
            var inventarios = await _context.Inventarios
                .Include(i => i.Ingrediente)
                .OrderBy(i => i.Ingrediente.Nombre_Ingrediente)
                .ToListAsync();

            var proveedoresPorIngrediente = await _context.Proveedores_Ingredientes
                .GroupBy(pi => pi.ID_Ingrediente)
                .Select(g => new { ID_Ingrediente = g.Key, Proveedores = g.Select(pi => pi.ID_Proveedor).ToList() })
                .ToListAsync();
            var mapaProveedores = proveedoresPorIngrediente.ToDictionary(x => x.ID_Ingrediente, x => x.Proveedores);

            return inventarios.Select(i => new IngredienteCompraVM
            {
                ID_Ingrediente = i.ID_Ingrediente,
                Nombre_Ingrediente = i.Ingrediente.Nombre_Ingrediente,
                Unidad_Medida = i.Ingrediente.Unidad_Medida,
                StockActual = i.Cantidad_Stock,
                CostoUnitario = i.Ingrediente.Costo_Unitario,
                ProveedoresIds = mapaProveedores.TryGetValue(i.ID_Ingrediente, out var provs) ? provs : new List<int>()
            }).ToList();
        }
    }
}