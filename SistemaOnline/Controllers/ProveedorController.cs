using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly APPDBContext _context;
        public ProveedorController(APPDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Proveedores.OrderBy(p => p.ID_Proveedor).Select(p => new ProveedorVM
            {
                ID_Proveedor = p.ID_Proveedor,
                Nombre_Empresa = p.Nombre_Empresa,
                RUC = p.RUC,
                Email_Contacto = p.Email_Contacto,
                Telefono = p.Telefono,
                Direccion = p.Direccion,
                Tipo_Suministro = p.Tipo_Suministro,
                Estado = p.Estado
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
            return View(new ProveedorVM());
        }
        [HttpPost]
        public async Task<IActionResult> Nuevo(ProveedorVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Proveedor proveedor = new Proveedor
            {
                Nombre_Empresa = modelo.Nombre_Empresa,
                RUC = modelo.RUC,
                Email_Contacto = modelo.Email_Contacto,
                Telefono = modelo.Telefono,
                Direccion = modelo.Direccion,
                Tipo_Suministro = modelo.Tipo_Suministro,
                Estado = modelo.Estado
            };
            await _context.Proveedores.AddAsync(proveedor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Proveedor proveedor = await _context.Proveedores.FirstAsync(p => p.ID_Proveedor == id);
            ProveedorVM modelo = new ProveedorVM
            {
                ID_Proveedor = proveedor.ID_Proveedor,
                Nombre_Empresa = proveedor.Nombre_Empresa,
                RUC = proveedor.RUC,
                Email_Contacto = proveedor.Email_Contacto,
                Telefono = proveedor.Telefono,
                Direccion = proveedor.Direccion,
                Tipo_Suministro = proveedor.Tipo_Suministro,
                Estado = proveedor.Estado
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(ProveedorVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Proveedor proveedor = await _context.Proveedores.FirstAsync(p => p.ID_Proveedor == modelo.ID_Proveedor);
            proveedor.Nombre_Empresa = modelo.Nombre_Empresa;
            proveedor.RUC = modelo.RUC;
            proveedor.Email_Contacto = modelo.Email_Contacto;
            proveedor.Telefono = modelo.Telefono;
            proveedor.Direccion = modelo.Direccion;
            proveedor.Tipo_Suministro = modelo.Tipo_Suministro;
            proveedor.Estado = modelo.Estado;
            _context.Proveedores.Update(proveedor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (await _context.Contratos.AnyAsync(c => c.ID_Proveedor == id))
            {
                TempData["Error"] = "No se puede eliminar un proveedor que tiene contratos asociados.";
                return RedirectToAction(nameof(Lista));
            }
            Proveedor proveedor = await _context.Proveedores.FirstAsync(p => p.ID_Proveedor == id);
            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
    }
}