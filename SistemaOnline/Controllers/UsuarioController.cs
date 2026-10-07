using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly APPDBContext _context;
        public UsuarioController(APPDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Usuarios.Include(u => u.Rol).OrderBy(u => u.ID_Usuario).Select(u => new UsuarioVM
            {
                ID_Usuario = u.ID_Usuario,
                Nombre_Usuario = u.Nombre_Usuario,
                Email = u.Email,
                Password = u.Password,
                Estado = u.Estado,
                ID_Rol = u.ID_Rol,
                RolNombre = u.Rol.Nombre_Rol
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
            UsuarioVM modelo = new UsuarioVM
            {
                RolesDisponibles = await ObtenerRoles()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(UsuarioVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.RolesDisponibles = await ObtenerRoles();
                return View(modelo);
            }

            Usuario usuario = new Usuario
            {
                Nombre_Usuario = modelo.Nombre_Usuario,
                Email = modelo.Email,
                Password = modelo.Password,
                Estado = modelo.Estado,
                ID_Rol = modelo.ID_Rol
            };
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();

            var rolCreado = await _context.Roles.FindAsync(usuario.ID_Rol);
            AuditoriaStore.Registrar(User, "Creación", "Usuario",
                $"Se creó el usuario '{usuario.Nombre_Usuario}' ({usuario.Email}) con rol '{rolCreado?.Nombre_Rol}'.");

            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Usuario usuario = await _context.Usuarios.FirstAsync(u => u.ID_Usuario == id);
            UsuarioVM modelo = new UsuarioVM
            {
                ID_Usuario = usuario.ID_Usuario,
                Nombre_Usuario = usuario.Nombre_Usuario,
                Email = usuario.Email,
                Password = usuario.Password,
                Estado = usuario.Estado,
                ID_Rol = usuario.ID_Rol,
                RolesDisponibles = await ObtenerRoles()
            };
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(UsuarioVM modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.RolesDisponibles = await ObtenerRoles();
                return View(modelo);
            }

            Usuario usuario = await _context.Usuarios.FirstAsync(u => u.ID_Usuario == modelo.ID_Usuario);
            int rolAnteriorId = usuario.ID_Rol;
            bool estadoAnterior = usuario.Estado;
            usuario.Nombre_Usuario = modelo.Nombre_Usuario;
            usuario.Email = modelo.Email;
            usuario.Password = modelo.Password;
            usuario.Estado = modelo.Estado;
            usuario.ID_Rol = modelo.ID_Rol;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            var detalles = new List<string>();
            if (rolAnteriorId != modelo.ID_Rol)
            {
                var rolAnterior = await _context.Roles.FindAsync(rolAnteriorId);
                var rolNuevo = await _context.Roles.FindAsync(modelo.ID_Rol);
                detalles.Add($"rol '{rolAnterior?.Nombre_Rol}' → '{rolNuevo?.Nombre_Rol}'");
            }
            if (estadoAnterior != modelo.Estado)
            {
                detalles.Add($"estado {(estadoAnterior ? "Activo" : "Inactivo")} → {(modelo.Estado ? "Activo" : "Inactivo")}");
            }
            string detalleCambios = detalles.Any() ? string.Join("; ", detalles) : "datos generales actualizados";
            AuditoriaStore.Registrar(User, "Edición", "Usuario",
                $"Se editó el usuario '{usuario.Nombre_Usuario}' ({usuario.Email}): {detalleCambios}.");

            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (id == 1)
            {
                TempData["Error"] = "No se puede eliminar la cuenta de administrador principal.";
                return RedirectToAction(nameof(Lista));
            }
            Usuario usuario = await _context.Usuarios.FirstAsync(u => u.ID_Usuario == id);
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            AuditoriaStore.Registrar(User, "Eliminación", "Usuario",
                $"Se eliminó el usuario '{usuario.Nombre_Usuario}' ({usuario.Email}).");

            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerRoles()
        {
            return await _context.Roles.Select(r => new SelectListItem
            {
                Value = r.ID_Rol.ToString(),
                Text = r.Nombre_Rol
            }).ToListAsync();
        }
    }
}