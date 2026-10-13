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
        public List<ContratoVM> AlertasContratos { get; set; } = new();
        public List<Inventario> InventarioCritico { get; set; } = new();
        public List<Pedido> PedidosRecientes { get; set; } = new();
        public List<EmpleadoVentaVM> TopEmpleados { get; set; } = new();
        public List<VentaMensualVM> VentasPorMes { get; set; } = new();
        public List<EstadoPedidoVM> PedidosPorEstado { get; set; } = new();

        // Productos más vendidos (CP-55): top 5 inicial sin filtros; los filtros
        // combinables se vuelven a pedir por AJAX a DashboardController.ProductosMasVendidos.
        public List<ProductoMasVendidoVM> ProductosMasVendidos { get; set; } = new();
        public List<Producto_Categoria> CategoriasDisponibles { get; set; } = new();
        public List<string> MetodosPagoDisponibles { get; set; } = new();
    }

    public class ProductoMasVendidoVM
    {
        public string Producto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
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