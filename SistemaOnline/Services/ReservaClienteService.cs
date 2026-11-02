using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using SistemaOnline.Data;
using SistemaOnline.Models;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Services
{
    // Lógica común de la reservación que hace un Cliente autenticado, usada por
    // KioscoController.Reservar y PortalClienteController.Reservar (antes duplicada).
    public static class ReservaClienteService
    {
        // Valida y registra la reservación. Devuelve la mesa reservada, o null si hubo
        // errores (quedan en modelState y modelo.Numero_Mesa se rellena para la vista).
        public static async Task<Mesa_Restaurante?> RegistrarAsync(APPDBContext db, KioscoReservaVM modelo,
            ClaimsPrincipal usuario, ModelStateDictionary modelState, string notaPorDefecto)
        {
            if (modelo.Fecha_Hora <= DateTime.Now)
            {
                modelState.AddModelError(nameof(modelo.Fecha_Hora), "La reservación debe ser para una fecha y hora futura.");
            }

            var mesa = await db.Mesas.FirstOrDefaultAsync(m => m.ID_Mesa == modelo.ID_Mesa);
            if (mesa == null || mesa.Estado != "Libre")
            {
                modelState.AddModelError(string.Empty, "Esa mesa ya no está disponible para reservar.");
            }

            int idUsuario = int.Parse(usuario.FindFirstValue("idUser")!);
            var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.ID_Usuario == idUsuario);
            if (cliente == null)
            {
                modelState.AddModelError(string.Empty, "Tu cuenta no tiene un perfil de cliente asociado; contacta al administrador.");
            }

            if (!modelState.IsValid)
            {
                modelo.Numero_Mesa = mesa?.Numero_Mesa ?? modelo.Numero_Mesa;
                return null;
            }

            Reservacion reservacion = new Reservacion
            {
                Fecha_Hora = modelo.Fecha_Hora!.Value,
                Numero_Personas = modelo.Numero_Personas,
                Ocasion_Especial = modelo.Ocasion_Especial,
                Estado_Reservacion = "Pendiente",
                Notas = string.IsNullOrWhiteSpace(modelo.Notas) ? notaPorDefecto : modelo.Notas,
                ID_Cliente = cliente!.ID_Cliente,
                ID_Mesa = modelo.ID_Mesa
            };
            await db.Reservaciones.AddAsync(reservacion);
            mesa!.Estado = "Reservada";
            await db.SaveChangesAsync();
            return mesa;
        }
    }
}
