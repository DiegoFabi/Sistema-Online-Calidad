using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Inventario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Inventario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Cantidad_Stock { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha_Ultima_Reposicion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Stock_Minimo { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Stock_Maximo { get; set; }

        // fk y objeto de relacion para ingrediente
        public int ID_Ingrediente { get; set; }
        public Ingrediente Ingrediente { get; set; }
    }
}