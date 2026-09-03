using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class Comprobante_PagoVM
    {
        public int ID_Comprobante { get; set; }

        // Toggle: si false, los campos de facturación no son requeridos
        public bool EmitirComprobante { get; set; } = false;

        [MaxLength(30)]
        public string? Tipo_Comprobante { get; set; }

        [MaxLength(10)]
        public string? Numero_Comprobante { get; set; }

        [MaxLength(10)]
        public string? Serie { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha_Emision { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El subtotal no puede ser negativo.")]
        public decimal Sub_Total { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto total no puede ser negativo.")]
        public decimal Monto_Total { get; set; }

        public decimal IGV { get; set; }

        [Required, MaxLength(30)]
        public string Estado_Comprobante { get; set; } = "Emitido";

        [Required, MaxLength(30)]
        public string Metodo_Pago { get; set; }

        // Facturación — solo requeridos si EmitirComprobante == true (validado en controller)
        [MaxLength(200)]
        public string? Razon_Social { get; set; }

        [MaxLength(11)]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "El RUC debe tener exactamente 11 dígitos numéricos.")]
        public string? RUC { get; set; }

        [MaxLength(200)]
        public string? Direccion_Fiscal { get; set; }

        [Required]
        public int ID_Pedido { get; set; }

        // Auxiliares para vistas
        public string? PedidoInfo { get; set; }
        public List<SelectListItem> PedidosDisponibles { get; set; } = new();

        public static List<SelectListItem> MetodosPago => new()
        {
            new SelectListItem("Efectivo", "Efectivo"),
            new SelectListItem("Yape", "Yape"),
            new SelectListItem("Plin", "Plin"),
            new SelectListItem("Tarjeta", "Tarjeta"),
        };
    }
}
