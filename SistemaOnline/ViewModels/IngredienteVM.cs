using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class IngredienteVM
    {
        public int ID_Ingrediente { get; set; }

        [Required, MaxLength(100)]
        public string Nombre_Ingrediente { get; set; }

        [Required, MaxLength(30)]
        public string Unidad_Medida { get; set; }

        [Required, MaxLength(200)]
        public string Descripcion { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor a 0.")]
        public decimal Costo_Unitario { get; set; }

        public bool Estado { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
        public int ID_Cat_Ingrediente { get; set; }

        // Para mostrar en Lista
        public string? CategoriaNombre { get; set; }

        // Para los selects en Nuevo/Editar
        public List<SelectListItem> CategoriasDisponibles { get; set; } = new();
    }
}
