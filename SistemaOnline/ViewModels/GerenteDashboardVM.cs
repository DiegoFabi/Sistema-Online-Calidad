using SistemaOnline.Models;

namespace SistemaOnline.ViewModels
{
    public class GerenteDashboardVM
    {
        public int ReservacionesHoy { get; set; }
        public int PedidosActivos { get; set; }
        public int MesasOcupadas { get; set; }
        public int ContratosPorVencer { get; set; }
        public List<Reservacion> ProximasReservaciones { get; set; } = new();
        public List<Turno> Turnos { get; set; } = new();
        public List<Contrato> AlertasContratos { get; set; } = new();
    }
}
