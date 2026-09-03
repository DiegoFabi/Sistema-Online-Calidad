using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class InventarioVM
    {
        public int ID_Inventario { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La cantidad en stock no puede ser negativa.")]
        public decimal Cantidad_Stock { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime Fecha_Ultima_Reposicion { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public decimal Stock_Minimo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El stock máximo no puede ser negativo.")]
        public decimal Stock_Maximo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un ingrediente válido.")]
        public int ID_Ingrediente { get; set; }

        // Para mostrar en Lista
        public string? IngredienteNombre { get; set; }

        // Para los selects en Nuevo/Editar
        public List<SelectListItem> IngredientesDisponibles { get; set; } = new();
    }
}