using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Producto_Ingrediente
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Producto_Ingrediente { get; set; }

        [Column(TypeName = "decimal(10,3)")]
        public decimal Cantidad { get; set; }

        [Required, MaxLength(100)]
        public string Unidad_Medida { get; set; }

        [Required, MaxLength(200)]
        public string Observaciones { get; set; }

        // fk y objeto de relacion para ingrediente
        public int ID_Ingrediente { get; set; }
        public Ingrediente Ingrediente { get; set; }

        // fk y objeto de relacion para producto
        public int ID_Producto { get; set; }
        public Producto Producto { get; set; }
    }
}