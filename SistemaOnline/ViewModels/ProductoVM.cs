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

        [Required, MaxLength(50)]
        public string Categoria { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
        public int ID_Categoria { get; set; }

        // Para mostrar en Lista
        public string? CategoriaRelNombre { get; set; }

        // Para los selects en Nuevo/Editar
        public List<SelectListItem> CategoriasDisponibles { get; set; } = new();
    }
}
