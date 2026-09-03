using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Contrato
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Contrato { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha_Inicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha_Fin { get; set; }

        [Required, MaxLength(50)]
        public string Tipo_Contrato { get; set; }

        [Range(100.01, double.MaxValue, ErrorMessage = "El salario debe ser mayor a 100.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Salario { get; set; }

        [Required, MaxLength(500)]
        public string Clausula { get; set; }

        // fk y objeto de relacion para empleado
        public int? ID_Empleado { get; set; }
        public Empleado? Empleado { get; set; }

        // fk y objeto de relacion para proveedor
        public int? ID_Proveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
    }
}