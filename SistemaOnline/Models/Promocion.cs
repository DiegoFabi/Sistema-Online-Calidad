using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Promocion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Promocion { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required, MaxLength(300)]
        public string Descripcion { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Porcentaje_Descuento { get; set; }

        public DateTime Fecha_Inicio { get; set; }

        public DateTime Fecha_Fin { get; set; }

        public bool Estado { get; set; }

        // una promocion puede aplicarse a muchos productos
        public ICollection<Producto_Promocion> Producto_Promociones { get; set; }
    }
}