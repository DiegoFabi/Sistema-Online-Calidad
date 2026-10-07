using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Services;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Controllers
{
    // Sin [Authorize] a nivel de clase: Reservaciones/EditarReservacion las puede usar
    // tambien el Mesero (ve y gestiona las reservas igual que el Administrador), el resto
    // de acciones de este controlador siguen siendo solo para el Administrador.
    public class AdministradorController : Controller
    {
        private readonly APPDBContext _dbcontext;
        public AdministradorController(APPDBContext dbContext)
        {
            _dbcontext = dbContext;
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);
            var limiteContrato = hoy.AddDays(30);

            var vm = new AdminDashboardVM
            {
                ReservacionesHoy = await _dbcontext.Reservaciones.CountAsync(r => r.Fecha_Hora >= hoy && r.Fecha_Hora < manana),
                PedidosActivos = await _dbcontext.Pedidos.CountAsync(p => p.Estado_Pedido != "Completado" && p.Estado_Pedido != "Pagado" && p.Estado_Pedido != "Cancelado"),
                MesasOcupadas = await _dbcontext.Mesas.CountAsync(m => m.Estado == "Ocupada"),
                ContratosPorVencer = await _dbcontext.Contratos_Empleados.CountAsync(c => c.Fecha_Fin >= hoy && c.Fecha_Fin <= limiteContrato)
                    + await _dbcontext.Contratos_Proveedores.CountAsync(c => c.Fecha_Fin >= hoy && c.Fecha_Fin <= limiteContrato)
            };

            vm.ProximasReservaciones = await _dbcontext.Reservaciones
                .Include(r => r.Cliente)
                .Include(r => r.Mesa_Restaurante)
                .Where(r => r.Fecha_Hora >= hoy)
                .OrderBy(r => r.Fecha_Hora)
                .Take(6)
                .ToListAsync();

            vm.Turnos = await _dbcontext.Turnos
                .Include(t => t.Empleado_Turnos)
                    .ThenInclude(et => et.Empleado)
                .ToListAsync();

            var alertasEmpleado = await _dbcontext.Contratos_Empleados
                .Include(c => c.Empleado)
                .Where(c => c.Fecha_Fin >= hoy && c.Fecha_Fin <= limiteContrato)
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

            var alertasProveedor = await _dbcontext.Contratos_Proveedores
                .Include(c => c.Proveedor)
                .Where(c => c.Fecha_Fin >= hoy && c.Fecha_Fin <= limiteContrato)
                .Select(c => new ContratoVM
                {
                    ID_Contrato = c.ID_Contrato_Proveedor,
                    Fecha_Inicio = c.Fecha_Inicio,
                    Fecha_Fin = c.Fecha_Fin,
                    Tipo_Contrato = c.Tipo_Contrato,
                    Clausula = c.Clausula,
                    TipoParticipante = "Proveedor",
                    ID_Proveedor = c.ID_Proveedor,
                    ProveedorNombre = c.Proveedor.Nombre_Empresa
                }).ToListAsync();

            vm.AlertasContratos = alertasEmpleado.Concat(alertasProveedor)
                .OrderBy(c => c.Fecha_Fin)
                .ToList();

            return View(vm);
        }

        [Authorize(Roles = "Administrador,Mesero")]
        public async Task<IActionResult> Reservaciones(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Reservaciones
                .Include(r => r.Cliente)
                .Include(r => r.Mesa_Restaurante)
                .OrderBy(r => r.Fecha_Hora);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            ViewBag.ClientesDisponibles = await ObtenerClientes();
            ViewBag.MesasDisponibles = await ObtenerMesas();
            return View(resultado.Items);
        }

        // Edicion completa de una reservacion desde el boton de editar de la lista (modal).
        // Mantiene el Estado de la Mesa consistente con el mismo criterio que ya usa
        // ReservacionController al editar una reservacion.
        [Authorize(Roles = "Administrador,Mesero")]
        [HttpPost]
        public async Task<IActionResult> EditarReservacion(ReservacionVM modelo)
        {
            var estadosActivos = new[] { "Pendiente", "Confirmada" };

            var reservacion = await _dbcontext.Reservaciones.FirstOrDefaultAsync(r => r.ID_Reservacion == modelo.ID_Reservacion);
            if (reservacion == null || !await _dbcontext.Clientes.AnyAsync(c => c.ID_Cliente == modelo.ID_Cliente)
                || !await _dbcontext.Mesas.AnyAsync(m => m.ID_Mesa == modelo.ID_Mesa))
            {
                TempData["Error"] = "No se pudo actualizar la reservación: datos inválidos.";
                return RedirectToAction(nameof(Reservaciones));
            }

            int mesaAnterior = reservacion.ID_Mesa;
            reservacion.Fecha_Hora = modelo.Fecha_Hora;
            reservacion.Numero_Personas = modelo.Numero_Personas;
            reservacion.Ocasion_Especial = modelo.Ocasion_Especial;
            reservacion.Estado_Reservacion = modelo.Estado_Reservacion;
            reservacion.Notas = modelo.Notas;
            reservacion.ID_Cliente = modelo.ID_Cliente;
            reservacion.ID_Mesa = modelo.ID_Mesa;
            await _dbcontext.SaveChangesAsync();

            if (mesaAnterior != modelo.ID_Mesa)
            {
                await LiberarMesaSiSinReservasActivas(mesaAnterior, estadosActivos);
            }
            if (estadosActivos.Contains(modelo.Estado_Reservacion))
            {
                var mesaNueva = await _dbcontext.Mesas.FindAsync(modelo.ID_Mesa);
                if (mesaNueva != null && mesaNueva.Estado == "Libre")
                {
                    mesaNueva.Estado = "Reservada";
                    await _dbcontext.SaveChangesAsync();
                }
            }
            else
            {
                await LiberarMesaSiSinReservasActivas(modelo.ID_Mesa, estadosActivos);
            }

            TempData["Exito"] = "La reservación se actualizó correctamente.";
            return RedirectToAction(nameof(Reservaciones));
        }

        private async Task LiberarMesaSiSinReservasActivas(int mesaId, string[] estadosActivos)
        {
            bool tieneOtrasActivas = await _dbcontext.Reservaciones
                .AnyAsync(r => r.ID_Mesa == mesaId && estadosActivos.Contains(r.Estado_Reservacion));
            if (tieneOtrasActivas)
            {
                return;
            }
            var mesa = await _dbcontext.Mesas.FindAsync(mesaId);
            if (mesa != null && mesa.Estado == "Reservada")
            {
                mesa.Estado = "Libre";
                await _dbcontext.SaveChangesAsync();
            }
        }

        private async Task<List<SelectListItem>> ObtenerClientes()
        {
            return await _dbcontext.Clientes.Select(c => new SelectListItem
            {
                Value = c.ID_Cliente.ToString(),
                Text = c.Nombre + " " + c.Apellidos
            }).ToListAsync();
        }

        private async Task<List<SelectListItem>> ObtenerMesas()
        {
            return await _dbcontext.Mesas.Select(m => new SelectListItem
            {
                Value = m.ID_Mesa.ToString(),
                Text = "Mesa " + m.Numero_Mesa
            }).ToListAsync();
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Turnos(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var query = _dbcontext.Turnos
                .Include(t => t.Empleado_Turnos)
                    .ThenInclude(et => et.Empleado)
                .OrderBy(t => t.ID_Turno);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Auditoria(string? entidad)
        {
            ViewBag.EntidadSeleccionada = entidad;
            ViewBag.EntidadesDisponibles = AuditoriaStore.ObtenerEntidades();
            return View(AuditoriaStore.Obtener(entidad));
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Pedidos(int page = 1, int pageSize = PaginationExtensions.DefaultPageSize)
        {
            var queryBase = _dbcontext.Pedidos
                .Where(p => p.Estado_Pedido != "Completado" && p.Estado_Pedido != "Pagado" && p.Estado_Pedido != "Cancelado");

            ViewBag.TotalPedidos = await queryBase.CountAsync();
            ViewBag.MesasOcupadas = await queryBase.Select(p => p.ID_Mesa).Distinct().CountAsync();
            ViewBag.Pendientes = await queryBase.CountAsync(p => p.Estado_Pedido == "Pendiente");
            ViewBag.Servidos = await queryBase.CountAsync(p => p.Estado_Pedido == "Servido");

            var query = queryBase
                .Include(p => p.Mesa_Restaurante)
                .Include(p => p.Empleado)
                .Include(p => p.Pedido_Detalles)
                    .ThenInclude(pd => pd.Producto)
                .OrderByDescending(p => p.ID_Pedido);

            var resultado = await query.ToPagedListAsync(page, pageSize);
            ViewBag.Page = resultado.Page;
            ViewBag.PageSize = resultado.PageSize;
            ViewBag.TotalPages = resultado.TotalPages;
            ViewBag.TotalCount = resultado.TotalCount;
            return View(resultado.Items);
        }
    }
}