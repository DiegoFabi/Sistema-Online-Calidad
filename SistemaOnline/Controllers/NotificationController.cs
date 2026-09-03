using Microsoft.AspNetCore.Mvc;
using SistemaOnline.Services;

namespace SistemaOnline.Controllers
{
    public class NotificacionController : Controller
    {
        [HttpGet]
        public IActionResult Listar()
        {
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