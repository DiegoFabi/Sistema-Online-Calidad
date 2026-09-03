using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;
using SistemaOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaOnline.Controllers
{
    public class ContratoController : Controller
    {
        private readonly APPDBContext _context;
        public ContratoController(APPDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _context.Contratos.Include(c => c.Empleado).Include(c => c.Proveedor).OrderBy(c => c.ID_Contrato).Select(c => new ContratoVM
            {
                ID_Contrato = c.ID_Contrato,
                Fecha_Inicio = c.Fecha_Inicio,
                Fecha_Fin = c.Fecha_Fin,
                Tipo_Contrato = c.Tipo_Contrato,
                Salario = c.Salario,
                Clausula = c.Clausula,
                TipoParticipante = c.ID_Empleado != null ? "Empleado" : "Proveedor",
                ID_Empleado = c.ID_Empleado,
                ID_Proveedor = c.ID_Proveedor,
                EmpleadoNombre = c.Empleado != null ? $"{c.Empleado.Nombre} {c.Empleado.Apellidos}" : null,
                ProveedorNombre = c.Proveedor != null ? c.Proveedor.Nombre_Empresa : null
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
            if (!await _context.Empleados.AnyAsync() && !await _context.Proveedores.AnyAsync())
            {
                ViewData["Msg"] = "Debes registrar al menos un Empleado o un Proveedor antes de crear un Contrato.";
                return View("~/Views/Negocio/Advertencia.cshtml");
            }

            ContratoVM modelo = new ContratoVM
            {
                Fecha_Inicio = DateTime.Now,
                EmpleadosDisponibles = await ObtenerEmpleados(),
                ProveedoresDisponibles = await ObtenerProveedores()
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Nuevo(ContratoVM modelo)
        {
            if (modelo.Fecha_Fin < modelo.Fecha_Inicio)
                ModelState.AddModelError(nameof(modelo.Fecha_Fin), "La fecha de fin debe ser posterior a la fecha de inicio.");

            ModelState.Remove(nameof(modelo.ID_Empleado));
            ModelState.Remove(nameof(modelo.ID_Proveedor));

            if (modelo.TipoParticipante == "Empleado")
            {
                modelo.ID_Proveedor = null;
                if (modelo.ID_Empleado == null || !await _context.Empleados.AnyAsync(e => e.ID_Empleado == modelo.ID_Empleado))
                    ModelState.AddModelError(nameof(modelo.ID_Empleado), "Selecciona un empleado válido.");
            }
            else
            {
                modelo.ID_Empleado = null;
                if (modelo.ID_Proveedor == null || !await _context.Proveedores.AnyAsync(p => p.ID_Proveedor == modelo.ID_Proveedor))
                    ModelState.AddModelError(nameof(modelo.ID_Proveedor), "Selecciona un proveedor válido.");
            }

            if (!ModelState.IsValid)
            {
                modelo.EmpleadosDisponibles = await ObtenerEmpleados();
                modelo.ProveedoresDisponibles = await ObtenerProveedores();
                return View(modelo);
            }

            Contrato contrato = new Contrato
            {
                Fecha_Inicio = modelo.Fecha_Inicio,
                Fecha_Fin = modelo.Fecha_Fin,
                Tipo_Contrato = modelo.Tipo_Contrato,
                Salario = modelo.Salario,
                Clausula = modelo.Clausula,
                ID_Empleado = modelo.ID_Empleado,
                ID_Proveedor = modelo.ID_Proveedor
            };
            await _context.Contratos.AddAsync(contrato);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Contrato contrato = await _context.Contratos.FirstAsync(c => c.ID_Contrato == id);
            ContratoVM modelo = new ContratoVM
            {
                ID_Contrato = contrato.ID_Contrato,
                Fecha_Inicio = contrato.Fecha_Inicio,
                Fecha_Fin = contrato.Fecha_Fin,
                Tipo_Contrato = contrato.Tipo_Contrato,
                Salario = contrato.Salario,
                Clausula = contrato.Clausula,
                ID_Empleado = contrato.ID_Empleado,
                ID_Proveedor = contrato.ID_Proveedor,
                TipoParticipante = contrato.ID_Empleado != null ? "Empleado" : "Proveedor",
                EmpleadosDisponibles = await ObtenerEmpleados(),
                ProveedoresDisponibles = await ObtenerProveedores()
            };
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(ContratoVM modelo)
        {
            if (modelo.Fecha_Fin < modelo.Fecha_Inicio)
                ModelState.AddModelError(nameof(modelo.Fecha_Fin), "La fecha de fin debe ser posterior a la fecha de inicio.");

            ModelState.Remove(nameof(modelo.ID_Empleado));
            ModelState.Remove(nameof(modelo.ID_Proveedor));

            if (modelo.TipoParticipante == "Empleado")
            {
                modelo.ID_Proveedor = null;
                if (modelo.ID_Empleado == null || !await _context.Empleados.AnyAsync(e => e.ID_Empleado == modelo.ID_Empleado))
                    ModelState.AddModelError(nameof(modelo.ID_Empleado), "Selecciona un empleado válido.");
            }
            else
            {
                modelo.ID_Empleado = null;
                if (modelo.ID_Proveedor == null || !await _context.Proveedores.AnyAsync(p => p.ID_Proveedor == modelo.ID_Proveedor))
                    ModelState.AddModelError(nameof(modelo.ID_Proveedor), "Selecciona un proveedor válido.");
            }

            if (!ModelState.IsValid)
            {
                modelo.EmpleadosDisponibles = await ObtenerEmpleados();
                modelo.ProveedoresDisponibles = await ObtenerProveedores();
                return View(modelo);
            }

            Contrato contrato = await _context.Contratos.FirstAsync(c => c.ID_Contrato == modelo.ID_Contrato);
            contrato.Fecha_Inicio = modelo.Fecha_Inicio;
            contrato.Fecha_Fin = modelo.Fecha_Fin;
            contrato.Tipo_Contrato = modelo.Tipo_Contrato;
            contrato.Salario = modelo.Salario;
            contrato.Clausula = modelo.Clausula;
            contrato.ID_Empleado = modelo.ID_Empleado;
            contrato.ID_Proveedor = modelo.ID_Proveedor;
            _context.Contratos.Update(contrato);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }
        [HttpGet]
        public async Task<ActionResult> Eliminar(int id)
        {
            Contrato contrato = await _context.Contratos.FirstAsync(c => c.ID_Contrato == id);
            _context.Contratos.Remove(contrato);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        private async Task<List<SelectListItem>> ObtenerEmpleados()
        {
            return await _context.Empleados.Select(e => new SelectListItem
            {
                Value = e.ID_Empleado.ToString(),
                Text = e.Nombre + " " + e.Apellidos
            }).ToListAsync();
        }

        private async Task<List<SelectListItem>> ObtenerProveedores()
        {
            return await _context.Proveedores.Select(p => new SelectListItem
            {
                Value = p.ID_Proveedor.ToString(),
                Text = p.Nombre_Empresa
            }).ToListAsync();
        }
    }
}