using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class ProveedorVM
    {
        public int ID_Proveedor { get; set; }

        [Required, MaxLength(200)]
        public string Nombre_Empresa { get; set; }

        [Required, MaxLength(11)]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "El RUC debe tener exactamente 11 dígitos numéricos.")]
        public string RUC { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string Email_Contacto { get; set; }

        [MaxLength(9)]
        [RegularExpression(@"^9[0-9]{8}$", ErrorMessage = "El teléfono debe iniciar con 9 y tener exactamente 9 dígitos.")]
        public string? Telefono { get; set; }

        [Required, MaxLength(250)]
        public string Direccion { get; set; }

        [Required, MaxLength(200)]
        public string Tipo_Suministro { get; set; }

        [Required, MaxLength(50)]
        public string Estado { get; set; }
    }
}
