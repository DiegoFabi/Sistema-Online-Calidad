using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class RegistrarCompraVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un proveedor.")]
        public int ID_Proveedor { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un ingrediente.")]
        public int ID_Ingrediente { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "La cantidad comprada debe ser mayor a 0.")]
        public decimal CantidadComprada { get; set; }

        public List<SelectListItem> ProveedoresDisponibles { get; set; } = new();
        public List<SelectListItem> IngredientesDisponibles { get; set; } = new();
    }
}