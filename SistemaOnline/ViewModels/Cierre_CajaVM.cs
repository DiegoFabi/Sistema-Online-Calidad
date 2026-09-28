using SistemaOnline.Models;
using SistemaOnline.Services;

namespace SistemaOnline.ViewModels
{
    public class Cierre_CajaVM
    {
        public DateTime Fecha { get; set; }
        public decimal TotalGeneral { get; set; }
        public int CantidadPagos { get; set; }
        public List<MetodoPagoVM> MetodosPago { get; set; } = new();
        public List<Pago> Pagos { get; set; } = new();

        public decimal EfectivoEsperado { get; set; }
        public RegistroCierreCaja? CierreRegistrado { get; set; }
    }
}