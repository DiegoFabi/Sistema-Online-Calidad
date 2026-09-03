using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class EmpleadoVM
    {
        public int ID_Empleado { get; set; }

        [Required, MaxLength(50)]
        public string Nombre { get; set; }

        [Required, MaxLength(50)]
        public string Apellidos { get; set; }

        [Required, MaxLength(100)]
        public string Direccion { get; set; }

        [Required, MaxLength(100)]
        public string Cargo { get; set; }

        [MaxLength(9)]
        [RegularExpression(@"^9[0-9]{8}$", ErrorMessage = "El teléfono debe iniciar con 9 y tener exactamente 9 dígitos.")]
        public string? Telefono { get; set; }

        [Required, MaxLength(15)]
        public string Estado { get; set; }

        [Required, MaxLength(8)]
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos numéricos.")]
        public string DNI { get; set; }

        public int? ID_Usuario { get; set; }

        // Para mostrar en Lista
        public string? UsuarioNombre { get; set; }

        // Para los selects en Nuevo/Editar
        public int? ID_Turno { get; set; }
        public List<SelectListItem> UsuariosDisponibles { get; set; } = new();
        public List<SelectListItem> TurnosDisponibles { get; set; } = new();
        public List<SelectListItem> CargosDisponibles { get; set; } = new();
    }
}
