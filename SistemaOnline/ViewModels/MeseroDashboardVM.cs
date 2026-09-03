using SistemaOnline.Models;

namespace SistemaOnline.ViewModels
{
    public class MeseroDashboardVM
    {
        public List<Mesa_Restaurante> Mesas { get; set; } = new();
        public int MesasLibres { get; set; }
        public int MesasOcupadas { get; set; }
        public int PedidosActivos { get; set; }
        public List<Pedido> PedidosListos { get; set; } = new();
        public HashSet<int> MesasConPedidoActivo { get; set; } = new();
    }
}
