using SistemaOnline.Models;

namespace SistemaOnline.ViewModels
{
    public class CocinaDashboardVM
    {
        public List<Pedido> Pendientes { get; set; } = new();
        public List<Pedido> Preparando { get; set; } = new();
        public List<Pedido> Listos { get; set; } = new();
        public int AlertasInventario { get; set; }
    }
}
