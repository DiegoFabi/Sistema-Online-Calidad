using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class Categoria_IngredienteVM
    {
        public int ID_Cat_Ingrediente { get; set; }

        [Required, MaxLength(50)]
        public string Nombre_Categoria { get; set; }
    }
}
