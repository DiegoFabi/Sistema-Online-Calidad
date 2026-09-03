using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class PagoVM
    {
        public int ID_Pago { get; set; }

        [Required(ErrorMessage = "La fecha y hora del pago son obligatorias.")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha_Hora_Pago { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 1.")]
        public decimal Monto { get; set; }

        [Required, MaxLength(50)]
        public string Metodo_Pago { get; set; }

        [MaxLength(100)]
        public string? Detalles_Tarjeta { get; set; }

        [Required, MaxLength(20)]
        public string Estado { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un pedido válido.")]
        public int ID_Pedido { get; set; }

        // Para mostrar en Lista
        public string? PedidoInfo { get; set; }

        // Para los selects en Nuevo/Editar
        public List<SelectListItem> PedidosDisponibles { get; set; } = new();
    }
}