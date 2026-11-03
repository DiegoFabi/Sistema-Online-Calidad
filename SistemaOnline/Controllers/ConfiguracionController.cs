using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaOnline.Services;

namespace SistemaOnline.Controllers
{
    // Configuración general del sistema: por ahora, el logo personalizable.
    public class ConfiguracionController : Controller
    {
        private readonly LogoService _logo;

        public ConfiguracionController(LogoService logo)
        {
            _logo = logo;
        }

        // Imagen del logo actual (lo usan el menú lateral y el portal del cliente).
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Logo()
        {
            var ruta = _logo.RutaActual();
            if (ruta == null) return NotFound();
            Response.Headers.CacheControl = "public, max-age=86400";
            return PhysicalFile(ruta, LogoService.TipoContenido(ruta));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(LogoService.TamanoMaximoBytes + 64 * 1024)]
        public async Task<IActionResult> SubirLogo(IFormFile? logo, string? volverA)
        {
            var error = await _logo.GuardarAsync(logo);
            if (error == null)
            {
                TempData["LogoExito"] = "El logo se actualizó correctamente.";
                AuditoriaStore.Registrar(User, "Edición", "Configuración", "Se actualizó el logo del sistema.");
            }
            else
            {
                TempData["LogoError"] = error;
            }
            return Volver(volverA);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarLogo(string? volverA)
        {
            _logo.Eliminar();
            TempData["LogoExito"] = "Se quitó el logo del sistema.";
            AuditoriaStore.Registrar(User, "Eliminación", "Configuración", "Se quitó el logo del sistema.");
            return Volver(volverA);
        }

        // Vuelve a la página desde la que se abrió Configuración (solo rutas locales).
        private IActionResult Volver(string? volverA) =>
            !string.IsNullOrEmpty(volverA) && Url.IsLocalUrl(volverA)
                ? LocalRedirect(volverA)
                : RedirectToAction("Index", "Dashboard");
    }
}