using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Categoria_Ingrediente
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Cat_Ingrediente { get; set; }

        [Required, MaxLength(50)]
        public string Nombre_Categoria { get; set; }

        // una categoria agrupa a muchos ingredientes
        public ICollection<Ingrediente> Ingredientes { get; set; }
    }
}