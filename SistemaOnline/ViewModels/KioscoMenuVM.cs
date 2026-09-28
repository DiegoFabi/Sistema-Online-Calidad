using SistemaOnline.Models;

namespace SistemaOnline.ViewModels
{
    public class KioscoMenuVM
    {
        public List<Producto_Categoria> Categorias { get; set; } = new();
        public int? CategoriaSeleccionada { get; set; }
        public List<Producto> Productos { get; set; } = new();
    }
}