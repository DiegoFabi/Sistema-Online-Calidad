using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Producto_Categoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Categoria { get; set; }

        [Required, MaxLength(50)]
        public string Nombre_Categoria { get; set; }

        [Required, MaxLength(200)]
        public string Descripcion { get; set; }

        // fk y objeto de relacion para carta
        public int ID_Carta { get; set; }
        public Carta Carta { get; set; }

        // una categoria tiene muchos productos
        public ICollection<Producto> Productos { get; set; }
    }
}