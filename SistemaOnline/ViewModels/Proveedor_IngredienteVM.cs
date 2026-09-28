using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class Proveedor_IngredienteVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un proveedor.")]
        public int ID_Proveedor { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un ingrediente.")]
        public int ID_Ingrediente { get; set; }

        // Para mostrar en Lista
        public string? ProveedorNombre { get; set; }
        public string? IngredienteNombre { get; set; }
        public List<SelectListItem> ProveedoresDisponibles { get; set; } = new();
        public List<SelectListItem> IngredientesDisponibles { get; set; } = new();
    }
}