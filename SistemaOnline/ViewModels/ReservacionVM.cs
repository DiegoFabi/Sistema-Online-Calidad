using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class ReservacionVM
    {
        public int ID_Reservacion { get; set; }

        [Required(ErrorMessage = "La fecha y hora de la reservación son obligatorias.")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha_Hora { get; set; }

        [Range(1, 50, ErrorMessage = "El número de personas debe ser entre 1 y 50.")]
        public int Numero_Personas { get; set; }

        [Required, MaxLength(100)]
        public string Ocasion_Especial { get; set; }

        [Required, MaxLength(20)]
        public string Estado_Reservacion { get; set; }

        [MaxLength(300)]
        public string? Notas { get; set; }

        [Required(ErrorMessage = "Selecciona un cliente.")]
        public int ID_Cliente { get; set; }

        [Required(ErrorMessage = "Selecciona una mesa.")]
        public int ID_Mesa { get; set; }

        // Para mostrar en Lista
        public string? ClienteNombre { get; set; }

        public string? MesaNumero { get; set; }

        // Para los selects en Nuevo/Editar
        public List<SelectListItem> ClientesDisponibles { get; set; } = new();

        public List<SelectListItem> MesasDisponibles { get; set; } = new();
    }
}
