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

        public int? ID_Turno { get; set; }
        public List<SelectListItem> UsuariosDisponibles { get; set; } = new();
        public List<SelectListItem> TurnosDisponibles { get; set; } = new();
        public List<SelectListItem> CargosDisponibles { get; set; } = new();

        [DataType(DataType.Date)]
        public DateTime Contrato_Fecha_Inicio { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime Contrato_Fecha_Fin { get; set; } = DateTime.Today.AddYears(1);

        [MaxLength(50)]
        public string Contrato_Tipo { get; set; } = "Indefinido";

        public decimal Contrato_Salario { get; set; }

        [MaxLength(500)]
        public string Contrato_Clausula { get; set; } =
            "Contrato de trabajo bajo el régimen general laboral, jornada de 48 horas semanales, con beneficios de ley (CTS, gratificaciones, EsSalud).";
    }
}