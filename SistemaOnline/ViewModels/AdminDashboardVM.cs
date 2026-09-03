using SistemaOnline.Models;

namespace SistemaOnline.ViewModels
{
    public class AdminDashboardVM
    {
        // Indicadores Operativos
        public int ReservacionesHoy { get; set; }
        public int PedidosActivos { get; set; }
        public int MesasOcupadas { get; set; }
        public int ContratosPorVencer { get; set; }

        // Indicadores Financieros
        public decimal VentasTotales { get; set; }
        public decimal VentasHoy { get; set; }
        public int ClientesTotales { get; set; }
        public int AlertasInventario { get; set; }

        // Listas de Datos
        public List<Reservacion> ProximasReservaciones { get; set; } = new();
        public List<Turno> Turnos { get; set; } = new();
        public List<Contrato> AlertasContratos { get; set; } = new();
        public List<Inventario> InventarioCritico { get; set; } = new();
        public List<Pedido> PedidosRecientes { get; set; } = new();
        public List<EmpleadoVentaVM> TopEmpleados { get; set; } = new();
        public List<VentaMensualVM> VentasPorMes { get; set; } = new();
        public List<EstadoPedidoVM> PedidosPorEstado { get; set; } = new();
    }

    public class VentaMensualVM
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class EmpleadoVentaVM
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public decimal TotalVendido { get; set; }
        public int CantidadPedidos { get; set; }
    }

    public class EstadoPedidoVM
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}