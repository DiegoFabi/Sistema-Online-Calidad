using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class PedidoVM
    {
        public int ID_Pedido { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Selecciona un estado para el pedido.")]
        public string Estado_Pedido { get; set; }

        [MaxLength(500)]
        public string? Detalle_Pedido { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El subtotal no puede ser negativo.")]
        public decimal SubTotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El total no puede ser negativo.")]
        public decimal Total { get; set; }

        [Required(ErrorMessage = "Selecciona un empleado.")]
        public int ID_Empleado { get; set; }

        [Required(ErrorMessage = "Selecciona una mesa.")]
        public int ID_Mesa { get; set; }

        // IDs de productos seleccionados en el modal de "Detalles del Pedido"
        public List<int> ProductosSeleccionados { get; set; } = new();

        // Cantidades por producto (ID_Producto → cantidad)
        public Dictionary<int, int> CantidadesProductos { get; set; } = new();

        // Para mostrar en Lista
        public string? EmpleadoNombre { get; set; }
        public string? MesaNumero { get; set; }

        // Para los selects en Nuevo/Editar
        public List<SelectListItem> EmpleadosDisponibles { get; set; } = new();
        public List<SelectListItem> MesasDisponibles { get; set; } = new();

        // Categorías con sus productos, para el modal "Detalles del Pedido"
        public List<CategoriaProductosVM> CategoriasProductos { get; set; } = new();
    }

    public class CategoriaProductosVM
    {
        public int ID_Categoria { get; set; }
        public string Nombre_Categoria { get; set; }
        public List<ProductoOpcionVM> Productos { get; set; } = new();
    }

    public class ProductoOpcionVM
    {
        public int ID_Producto { get; set; }
        public string Nombre_Plato { get; set; }
        public decimal Precio { get; set; }
    }
}