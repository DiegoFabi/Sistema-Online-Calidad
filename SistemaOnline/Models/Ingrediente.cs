using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Ingrediente
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Ingrediente { get; set; }

        [Required, MaxLength(100)]
        public string Nombre_Ingrediente { get; set; }

        [Required, MaxLength(30)]
        public string Unidad_Medida { get; set; }

        [Required, MaxLength(200)]
        public string Descripcion { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor a 0.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Costo_Unitario { get; set; }

        public bool Estado { get; set; }

        // fk y relacion para categoria_ingrediente
        public int ID_Cat_Ingrediente { get; set; }
        public Categoria_Ingrediente Categoria_Ingrediente { get; set; }

        // colecciones de relacion
        public ICollection<Producto_Ingrediente> Producto_Ingredientes { get; set; }
        public ICollection<Proveedor_Ingrediente> Proveedor_Ingredientes { get; set; }
        public ICollection<Inventario> Inventarios { get; set; }
    }
}