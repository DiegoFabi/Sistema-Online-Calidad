using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class ClienteVM
    {
        public int ID_Cliente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio."), MaxLength(50)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios."), MaxLength(50)]
        public string Apellidos { get; set; }

        [MaxLength(9)]
        [RegularExpression(@"^9[0-9]{8}$", ErrorMessage = "El teléfono debe iniciar con 9 y tener exactamente 9 dígitos.")]
        public string? Telefono { get; set; }

        [MaxLength(100), EmailAddress]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Fecha_Nacimiento { get; set; }

        [MaxLength(150)]
        public string? Direccion { get; set; }

        [MaxLength(8)]
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos numéricos.")]
        public string? DNI { get; set; }

        [MaxLength(11)]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "El RUC debe tener exactamente 11 dígitos numéricos.")]
        public string? RUC { get; set; }
    }
}
