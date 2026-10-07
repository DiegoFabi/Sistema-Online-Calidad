using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class RolController : Controller
    {
        private readonly APPDBContext _context;
        public RolController(APPDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Roles.OrderBy(r => r.ID_Rol).Select(r => new RolVM
            {
                ID_Rol = r.ID_Rol,
                Nombre_Rol = r.Nombre_Rol,
                Descripcion = r.Descripcion
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
            return View(new RolVM());
        }
        [HttpPost]
        public async Task<IActionResult> Nuevo(RolVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Rol rol = new Rol
            {
                Nombre_Rol = modelo.Nombre_Rol,
                Descripcion = modelo.Descripcion
            };
            await _context.Roles.AddAsync(rol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Rol rol = await _context.Roles.FirstAsync(r => r.ID_Rol == id);
            RolVM modelo = new RolVM
            {
                ID_Rol = rol.ID_Rol,
                Nombre_Rol = rol.Nombre_Rol,
                Descripcion = rol.Descripcion
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(RolVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            Rol rol = await _context.Roles.FirstAsync(r => r.ID_Rol == modelo.ID_Rol);
            rol.Nombre_Rol = modelo.Nombre_Rol;
            rol.Descripcion = modelo.Descripcion;
            _context.Roles.Update(rol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (await _context.Usuarios.AnyAsync(u => u.ID_Rol == id))
            {
                TempData["Error"] = "No se puede eliminar un rol que tiene usuarios asociados.";
                return RedirectToAction(nameof(Lista));
            }
            Rol rol = await _context.Roles.FirstAsync(r => r.ID_Rol == id);
            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
    }
}