using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace SistemaOnline.Models
{
    public class Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Proveedor { get; set; }

        [Required, MaxLength(200)]
        public string Nombre_Empresa { get; set; }

        [Required, MaxLength(11)]
        public string RUC { get; set; }

        [Required, MaxLength(100)]
        public string Email_Contacto { get; set; }

        [MaxLength(9)]
        [RegularExpression(@"^[0-9+\-() ]*$", ErrorMessage = "El teléfono solo puede contener números y los símbolos + - ( ).")]
        public string? Telefono { get; set; }

        [Required, MaxLength(250)]
        public string Direccion { get; set; }

        [Required, MaxLength(200)]
        public string Tipo_Suministro { get; set; }

        [Required, MaxLength(50)]
        public string Estado { get; set; }

        // Un proveedor tiene muchos contratos y proveen diferentes tipos de productos (ingredientes)
        public ICollection<Contrato> Contratos { get; set; }
        public ICollection<Proveedor_Ingrediente> Proveedor_Ingredientes { get; set; }
    }
}