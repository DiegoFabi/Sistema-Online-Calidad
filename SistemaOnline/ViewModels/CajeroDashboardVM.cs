using SistemaOnline.Models;

namespace SistemaOnline.ViewModels
{
    public class CajeroDashboardVM
    {
        public decimal TotalRecaudadoHoy { get; set; }
        public int PagosHoyCount { get; set; }
        public int PedidosPorCobrar { get; set; }
        public List<Pedido> PedidosPendientes { get; set; } = new();
        public List<Pago> PagosRecientes { get; set; } = new();
        public List<MetodoPagoVM> MetodosPago { get; set; } = new();
    }

    public class MetodoPagoVM
    {
        public string Metodo { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
    }
}
