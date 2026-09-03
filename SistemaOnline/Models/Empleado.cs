using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace SistemaOnline.Models
{
    public class Empleado
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        [RegularExpression(@"^[0-9+\-() ]*$", ErrorMessage = "El teléfono solo puede contener números y los símbolos + - ( ).")]
        public string? Telefono { get; set; }

        [Required, MaxLength(15)]
        public string Estado { get; set; }

        [Required, MaxLength(8)]
        public string DNI { get; set; }

        public int? ID_Usuario { get; set; }
        public Usuario? Usuario { get; set; }
        public ICollection<Contrato> Contratos { get; set; }
        public ICollection<Empleado_Turno> Empleado_Turnos { get; set; }
        public ICollection<Pedido> Pedidos { get; set; }
    }
}