using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Carta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Carta { get; set; }

        [Required, MaxLength(100)]
        public string Nombre_Carta { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad de platos no puede ser negativa.")]
        public int Cantidad_Platos { get; set; }

        [Required, MaxLength(100)]
        public string Descripcion { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 1.")]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Precio { get; set; }

        // una carta tiene muchas categorias de productos
        public ICollection<Producto_Categoria> Producto_Categorias { get; set; }
    }
}