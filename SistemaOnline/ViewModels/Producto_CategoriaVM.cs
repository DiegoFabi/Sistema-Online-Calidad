using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class Producto_CategoriaVM
    {
        public int ID_Categoria { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio"), MaxLength(50)]
        public string Nombre_Categoria { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria"), MaxLength(200)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una carta")]
        public int ID_Carta { get; set; }

        // Para mostrar en Lista
        public string? CartaNombre { get; set; }

        // Para los selects en Nuevo/Editar
        public List<SelectListItem> CartasDisponibles { get; set; } = new();
    }
}
