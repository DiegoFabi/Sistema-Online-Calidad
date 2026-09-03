using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Comprobante_Pago
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Comprobante { get; set; }

        [MaxLength(30)]
        public string? Tipo_Comprobante { get; set; }

        [MaxLength(10)]
        public string? Numero_Comprobante { get; set; }

        [MaxLength(10)]
        public string? Serie { get; set; }

        public DateTime Fecha_Emision { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 1.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Sub_Total { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El monto total debe ser mayor o igual a 1.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto_Total { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal IGV { get; set; }

        [Required, MaxLength(30)]
        public string Estado_Comprobante { get; set; }

        [Required, MaxLength(30)]
        public string Metodo_Pago { get; set; }

        [MaxLength(200)]
        public string? Razon_Social { get; set; }

        [MaxLength(11)]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "El RUC solo puede contener números.")]
        public string? RUC { get; set; }

        [MaxLength(200)]
        public string? Direccion_Fiscal { get; set; }

        // fk y objeto de relacion para pedido
        public int ID_Pedido { get; set; }
        public Pedido Pedido { get; set; }
    }
}