using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using System.Security.Claims;

namespace SistemaOnline.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class LoginController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public LoginController(APPDBContext dbContext)
        {
            _dbcontext = dbContext;
        }
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registro(UsuarioVM modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            if (modelo.Password != modelo.RepeatPassword)
            {
                ViewData["Msg"] = "La contraseñas no coinciden, escribe denuevo";
                return View();
            }
            var rolCliente = await _dbcontext.Roles.FirstOrDefaultAsync(r => r.Nombre_Rol == "Cliente");
            Usuario user = new Usuario()
            {
                Nombre_Usuario = modelo.Nombre_Usuario,
                Email = modelo.Email,
                Password = modelo.Password,
                ID_Rol = rolCliente.ID_Rol
            };
            await _dbcontext.Usuarios.AddAsync(user);
            await _dbcontext.SaveChangesAsync();
            if (user.ID_Usuario != 0) return RedirectToAction("Login", "Login");
            ViewData["Msg"] = "El usuario no se creo";
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            Usuario? existe = await _dbcontext.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(
                u => u.Email == modelo.Email &&
                u.Password == modelo.Password);

            if (existe == null)
            {
                ViewData["Msg"] = "El usuario no existe o la contraseña es incorrecta";
                return View();
            }

            if (!existe.Estado)
            {
                ViewData["Msg"] = "Tu cuenta está deshabilitada. Contacta al administrador.";
                return View();
            }
            //Claims
            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, existe.Nombre_Usuario),
                new Claim(ClaimTypes.Email, existe.Email),
                new Claim(ClaimTypes.Role, existe.Rol.Nombre_Rol),
                new Claim("idUser", existe.ID_Usuario.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);
            var propiedad = new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = true
            };
            await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            propiedad
            );
            //Sesiones por rol
            HttpContext.Session.SetString("User", existe.Nombre_Usuario);
            HttpContext.Session.SetString("Rol", existe.Rol.Nombre_Rol);
            HttpContext.Session.SetInt32("idUser", existe.ID_Usuario);
            switch (existe.Rol.Nombre_Rol)
            {
                case "Administrador":
                    return RedirectToAction("Index", "Administrador");
                case "Chef":
                    return RedirectToAction("Index", "Cocina");
                case "Mesero":
                    return RedirectToAction("Index", "Mesero");
                case "Cajero":
                    return RedirectToAction("Index", "Cajero");
                default:
                    return RedirectToAction("Index", "Cliente");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public IActionResult Administrador()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Mesero()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Cajero()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Cliente()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }

    }
}