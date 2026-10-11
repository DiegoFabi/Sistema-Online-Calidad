using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Services;

namespace SistemaOnline.Controllers
{
    public class NotificacionController : Controller
    {
        private readonly APPDBContext _context;
        public NotificacionController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            // Reconciliación defensiva: además de la notificación que ya dispara
            // CocinaController al marcar un pedido como "Listo", aquí se asegura (de forma
            // idempotente, vía AgregarUnaVez) que todo pedido que esté actualmente en estado
            // "Listo" tenga su notificación. Así, aunque hubiera varios pedidos listos a la
            // vez, cada uno aparece en la lista — y ninguno se pierde si el evento puntual no
            // llegó a generarse por cualquier motivo.
            var pedidosListos = await _context.Pedidos
                .Include(p => p.Mesa_Restaurante)
                .Where(p => p.Estado_Pedido == "Listo")
                .ToListAsync();

            foreach (var pedido in pedidosListos)
            {
                var mesa = pedido.Mesa_Restaurante != null ? $"Mesa {pedido.Mesa_Restaurante.Numero_Mesa}" : $"Mesa {pedido.ID_Mesa}";
                NotificacionStore.AgregarUnaVez($"pedido-listo-{pedido.ID_Pedido}", "notifications_active", "Pedido listo para servir", $"Pedido #{pedido.ID_Pedido} ({mesa}) está listo. El mesero debe recogerlo.");
            }

            return Json(NotificacionStore.ObtenerNoLeidas());
        }

        [HttpPost]
        public IActionResult MarcarLeida(int id)
        {
            NotificacionStore.MarcarLeida(id);
            return Ok();
        }
    }
}