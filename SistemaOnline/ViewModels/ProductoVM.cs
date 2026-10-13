using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class ProductoVM
    {
        public int ID_Producto { get; set; }

        [Required, MaxLength(100)]
        public string Nombre_Plato { get; set; }

        [Required, MaxLength(255)]
        public string Descripcion { get; set; }

        [Range(5, double.MaxValue, ErrorMessage = "El tiempo de preparación debe ser de al menos 5 minutos.")]
        public double Tiempo_Preparacion { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 1.")]
        public decimal Precio { get; set; }

        public bool Disponibilidad { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
        public int ID_Categoria { get; set; }

        // Para mostrar en Lista
        public string? CategoriaRelNombre { get; set; }

        // Para los selects en Nuevo/Editar
        public List<SelectListItem> CategoriasDisponibles { get; set; } = new();

        // IDs de ingredientes seleccionados en el modal "Ingredientes de la Receta"
        public List<int> IngredientesSeleccionados { get; set; } = new();

        // Cantidades por ingrediente (ID_Ingrediente → cantidad)
        public Dictionary<int, decimal> CantidadesIngredientes { get; set; } = new();

        // Categorías con sus ingredientes, para el modal "Ingredientes de la Receta"
        public List<CategoriaIngredientesVM> CategoriasIngredientes { get; set; } = new();
    }

    public class CategoriaIngredientesVM
    {
        public int ID_Cat_Ingrediente { get; set; }
        public string Nombre_Categoria { get; set; }
        public List<IngredienteOpcionVM> Ingredientes { get; set; } = new();
    }

    public class IngredienteOpcionVM
    {
        public int ID_Ingrediente { get; set; }
        public string Nombre_Ingrediente { get; set; }
        public string Unidad_Medida { get; set; }
    }
}
