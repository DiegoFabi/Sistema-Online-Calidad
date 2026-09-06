using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Contrato_Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Contrato_Proveedor { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha_Inicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha_Fin { get; set; }

        [Required, MaxLength(50)]
        public string Tipo_Contrato { get; set; }

        [Required, MaxLength(500)]
        public string Clausula { get; set; }

        // fk y objeto de relacion para proveedor
        public int ID_Proveedor { get; set; }
        public Proveedor Proveedor { get; set; }
    }
}