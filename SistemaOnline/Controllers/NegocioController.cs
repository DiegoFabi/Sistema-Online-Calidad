using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;

namespace SistemaOnline.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class NegocioController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public NegocioController(APPDBContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet]
        public IActionResult Advertencia()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Confirmar(bool aceptaTerminos)
        {
            if (!aceptaTerminos)
            {
                ViewData["Msg"] = "Debes aceptar los términos para continuar.";
                return View("Advertencia");
            }
            return RedirectToAction("Gestion");
        }

        [HttpGet]
        public async Task<IActionResult> Gestion()
        {
            var bloqueados = new Dictionary<string, string>();
            if (!await _dbcontext.Empleados.AnyAsync() && !await _dbcontext.Proveedores.AnyAsync())
                bloqueados["Contrato"] = "Para desbloquear esta sección, debe registrar al menos un Empleado o un Proveedor.";
            if (!await _dbcontext.Clientes.AnyAsync() || !await _dbcontext.Mesas.AnyAsync())
                bloqueados["Reservacion"] = "Para desbloquear esta sección, debe registrar al menos un Cliente y una Mesa.";
            if (!await _dbcontext.Empleados.AnyAsync() || !await _dbcontext.Mesas.AnyAsync())
                bloqueados["Pedido"] = "Para desbloquear esta sección, debe registrar al menos un Empleado y una Mesa.";
            if (!await _dbcontext.Cartas.AnyAsync())
                bloqueados["Producto_Categoria"] = "Para desbloquear esta sección, debe registrar al menos una Carta en el módulo Cartas.";
            if (!await _dbcontext.Productos_Categorias.AnyAsync())
                bloqueados["Producto"] = "Para desbloquear esta sección, debe registrar al menos una Categoría de Producto en el módulo Categorías de Producto.";
            if (!await _dbcontext.Categorias_Ingredientes.AnyAsync())
                bloqueados["Ingrediente"] = "Para desbloquear esta sección, debe registrar al menos una Categoría de Ingrediente en el módulo Categorías de Ingrediente.";
            if (!await _dbcontext.Ingredientes.AnyAsync())
                bloqueados["Inventario"] = "Para desbloquear esta sección, debe registrar al menos un Ingrediente en el módulo Ingredientes.";
            if (!await _dbcontext.Pedidos.AnyAsync())
            {
                bloqueados["Pago"] = "Para desbloquear esta sección, debe registrar al menos un Pedido en el módulo Pedidos.";
                bloqueados["Comprobante_Pago"] = "Para desbloquear esta sección, debe registrar al menos un Pedido en el módulo Pedidos.";
            }
            ViewBag.Bloqueados = bloqueados;
            return View();
        }
    }
}