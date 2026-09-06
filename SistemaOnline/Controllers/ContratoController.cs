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
            var contratosEmpleado = await _context.Contratos_Empleados
                .Include(c => c.Empleado)
                .Select(c => new ContratoVM
                {
                    ID_Contrato = c.ID_Contrato_Empleado,
                    Fecha_Inicio = c.Fecha_Inicio,
                    Fecha_Fin = c.Fecha_Fin,
                    Tipo_Contrato = c.Tipo_Contrato,
                    Salario = c.Salario,
                    Clausula = c.Clausula,
                    TipoParticipante = "Empleado",
                    ID_Empleado = c.ID_Empleado,
                    EmpleadoNombre = c.Empleado.Nombre + " " + c.Empleado.Apellidos
                }).ToListAsync();

            var contratosProveedor = await _context.Contratos_Proveedores
                .Include(c => c.Proveedor)
                .Select(c => new ContratoVM
                {
                    ID_Contrato = c.ID_Contrato_Proveedor,
                    Fecha_Inicio = c.Fecha_Inicio,
                    Fecha_Fin = c.Fecha_Fin,
                    Tipo_Contrato = c.Tipo_Contrato,
                    Salario = null,
                    Clausula = c.Clausula,
                    TipoParticipante = "Proveedor",
                    ID_Proveedor = c.ID_Proveedor,
                    ProveedorNombre = c.Proveedor.Nombre_Empresa
                }).ToListAsync();

            var todos = contratosEmpleado.Concat(contratosProveedor)
                .OrderBy(c => c.ID_Contrato)
                .ThenBy(c => c.TipoParticipante)
                .ToList();

            if (page < 1) page = 1;
            if (!PaginationExtensions.TamanosPaginaPermitidos.Contains(pageSize)) pageSize = PaginationExtensions.DefaultPageSize;

            int totalCount = todos.Count;
            int totalPages = pageSize <= 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var items = todos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            return View(items);
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
            ModelState.Remove(nameof(modelo.Salario));

            bool esEmpleado = modelo.TipoParticipante == "Empleado";

            if (esEmpleado)
            {
                modelo.ID_Proveedor = null;
                if (modelo.ID_Empleado == null || !await _context.Empleados.AnyAsync(e => e.ID_Empleado == modelo.ID_Empleado))
                    ModelState.AddModelError(nameof(modelo.ID_Empleado), "Selecciona un empleado válido.");
                if (modelo.Salario == null || modelo.Salario <= 100)
                    ModelState.AddModelError(nameof(modelo.Salario), "El salario debe ser mayor a 100.");
            }
            else
            {
                modelo.ID_Empleado = null;
                modelo.Salario = null;
                if (modelo.ID_Proveedor == null || !await _context.Proveedores.AnyAsync(p => p.ID_Proveedor == modelo.ID_Proveedor))
                    ModelState.AddModelError(nameof(modelo.ID_Proveedor), "Selecciona un proveedor válido.");
            }

            if (!ModelState.IsValid)
            {
                modelo.EmpleadosDisponibles = await ObtenerEmpleados();
                modelo.ProveedoresDisponibles = await ObtenerProveedores();
                return View(modelo);
            }

            if (esEmpleado)
            {
                Contrato_Empleado contrato = new Contrato_Empleado
                {
                    Fecha_Inicio = modelo.Fecha_Inicio,
                    Fecha_Fin = modelo.Fecha_Fin,
                    Tipo_Contrato = modelo.Tipo_Contrato,
                    Salario = modelo.Salario!.Value,
                    Clausula = modelo.Clausula,
                    ID_Empleado = modelo.ID_Empleado!.Value
                };
                await _context.Contratos_Empleados.AddAsync(contrato);
            }
            else
            {
                Contrato_Proveedor contrato = new Contrato_Proveedor
                {
                    Fecha_Inicio = modelo.Fecha_Inicio,
                    Fecha_Fin = modelo.Fecha_Fin,
                    Tipo_Contrato = modelo.Tipo_Contrato,
                    Clausula = modelo.Clausula,
                    ID_Proveedor = modelo.ID_Proveedor!.Value
                };
                await _context.Contratos_Proveedores.AddAsync(contrato);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string tipo)
        {
            ContratoVM modelo;
            if (tipo == "Proveedor")
            {
                Contrato_Proveedor contrato = await _context.Contratos_Proveedores.FirstAsync(c => c.ID_Contrato_Proveedor == id);
                modelo = new ContratoVM
                {
                    ID_Contrato = contrato.ID_Contrato_Proveedor,
                    Fecha_Inicio = contrato.Fecha_Inicio,
                    Fecha_Fin = contrato.Fecha_Fin,
                    Tipo_Contrato = contrato.Tipo_Contrato,
                    Clausula = contrato.Clausula,
                    ID_Proveedor = contrato.ID_Proveedor,
                    TipoParticipante = "Proveedor"
                };
            }
            else
            {
                Contrato_Empleado contrato = await _context.Contratos_Empleados.FirstAsync(c => c.ID_Contrato_Empleado == id);
                modelo = new ContratoVM
                {
                    ID_Contrato = contrato.ID_Contrato_Empleado,
                    Fecha_Inicio = contrato.Fecha_Inicio,
                    Fecha_Fin = contrato.Fecha_Fin,
                    Tipo_Contrato = contrato.Tipo_Contrato,
                    Salario = contrato.Salario,
                    Clausula = contrato.Clausula,
                    ID_Empleado = contrato.ID_Empleado,
                    TipoParticipante = "Empleado"
                };
            }
            modelo.EmpleadosDisponibles = await ObtenerEmpleados();
            modelo.ProveedoresDisponibles = await ObtenerProveedores();
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(ContratoVM modelo)
        {
            if (modelo.Fecha_Fin < modelo.Fecha_Inicio)
                ModelState.AddModelError(nameof(modelo.Fecha_Fin), "La fecha de fin debe ser posterior a la fecha de inicio.");

            ModelState.Remove(nameof(modelo.ID_Empleado));
            ModelState.Remove(nameof(modelo.ID_Proveedor));
            ModelState.Remove(nameof(modelo.Salario));

            bool esEmpleado = modelo.TipoParticipante == "Empleado";

            if (esEmpleado)
            {
                modelo.ID_Proveedor = null;
                if (modelo.ID_Empleado == null || !await _context.Empleados.AnyAsync(e => e.ID_Empleado == modelo.ID_Empleado))
                    ModelState.AddModelError(nameof(modelo.ID_Empleado), "Selecciona un empleado válido.");
                if (modelo.Salario == null || modelo.Salario <= 100)
                    ModelState.AddModelError(nameof(modelo.Salario), "El salario debe ser mayor a 100.");
            }
            else
            {
                modelo.ID_Empleado = null;
                modelo.Salario = null;
                if (modelo.ID_Proveedor == null || !await _context.Proveedores.AnyAsync(p => p.ID_Proveedor == modelo.ID_Proveedor))
                    ModelState.AddModelError(nameof(modelo.ID_Proveedor), "Selecciona un proveedor válido.");
            }

            if (!ModelState.IsValid)
            {
                modelo.EmpleadosDisponibles = await ObtenerEmpleados();
                modelo.ProveedoresDisponibles = await ObtenerProveedores();
                return View(modelo);
            }

            if (esEmpleado)
            {
                Contrato_Empleado contrato = await _context.Contratos_Empleados.FirstAsync(c => c.ID_Contrato_Empleado == modelo.ID_Contrato);
                contrato.Fecha_Inicio = modelo.Fecha_Inicio;
                contrato.Fecha_Fin = modelo.Fecha_Fin;
                contrato.Tipo_Contrato = modelo.Tipo_Contrato;
                contrato.Salario = modelo.Salario!.Value;
                contrato.Clausula = modelo.Clausula;
                contrato.ID_Empleado = modelo.ID_Empleado!.Value;
                _context.Contratos_Empleados.Update(contrato);
            }
            else
            {
                Contrato_Proveedor contrato = await _context.Contratos_Proveedores.FirstAsync(c => c.ID_Contrato_Proveedor == modelo.ID_Contrato);
                contrato.Fecha_Inicio = modelo.Fecha_Inicio;
                contrato.Fecha_Fin = modelo.Fecha_Fin;
                contrato.Tipo_Contrato = modelo.Tipo_Contrato;
                contrato.Clausula = modelo.Clausula;
                contrato.ID_Proveedor = modelo.ID_Proveedor!.Value;
                _context.Contratos_Proveedores.Update(contrato);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Lista));
        }

        [HttpGet]
        public async Task<ActionResult> Eliminar(int id, string tipo)
        {
            if (tipo == "Proveedor")
            {
                Contrato_Proveedor contrato = await _context.Contratos_Proveedores.FirstAsync(c => c.ID_Contrato_Proveedor == id);
                _context.Contratos_Proveedores.Remove(contrato);
            }
            else
            {
                Contrato_Empleado contrato = await _context.Contratos_Empleados.FirstAsync(c => c.ID_Contrato_Empleado == id);
                _context.Contratos_Empleados.Remove(contrato);
            }
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