using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class RegistrarCompraVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un proveedor.")]
        public int ID_Proveedor { get; set; }

        // Ingredientes elegidos en el modal (checkbox + cantidad), igual que el patrón ya
        // usado para la receta de un Producto o los productos de un Pedido.
        public List<int> IngredientesSeleccionados { get; set; } = new();
        public Dictionary<int, decimal> CantidadesIngredientes { get; set; } = new();

        public List<SelectListItem> ProveedoresDisponibles { get; set; } = new();

        // Ingredientes que se pueden solicitar, con su stock actual y los proveedores que ya
        // los suministran (Proveedores_Ingredientes), para que el modal se filtre según el
        // proveedor elegido sin ir al servidor.
        public List<IngredienteCompraVM> IngredientesParaComprar { get; set; } = new();
    }

    public class IngredienteCompraVM
    {
        public int ID_Ingrediente { get; set; }
        public string Nombre_Ingrediente { get; set; } = "";
        public string Unidad_Medida { get; set; } = "";
        public decimal StockActual { get; set; }
        // Costo_Unitario del Ingrediente, para calcular en el modal cuánto costaría la
        // solicitud según lo marcado (ya registrado en el Ingrediente, no es un dato nuevo).
        public decimal CostoUnitario { get; set; }
        public List<int> ProveedoresIds { get; set; } = new();
    }
}
