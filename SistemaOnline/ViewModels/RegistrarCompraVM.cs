using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class RegistrarCompraVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un proveedor.")]
        public int ID_Proveedor { get; set; }

        public List<int> IngredientesSeleccionados { get; set; } = new();
        public Dictionary<int, decimal> CantidadesIngredientes { get; set; } = new();

        public List<SelectListItem> ProveedoresDisponibles { get; set; } = new();

        public List<IngredienteCompraVM> IngredientesParaComprar { get; set; } = new();
    }

    public class IngredienteCompraVM
    {
        public int ID_Ingrediente { get; set; }
        public string Nombre_Ingrediente { get; set; } = "";
        public string Unidad_Medida { get; set; } = "";
        public decimal StockActual { get; set; }

        public decimal CostoUnitario { get; set; }
        public List<int> ProveedoresIds { get; set; } = new();
    }
}