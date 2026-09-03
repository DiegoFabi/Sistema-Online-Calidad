using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Pago
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Pago { get; set; }

        public DateTime Fecha_Hora_Pago { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 1.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        [Required, MaxLength(50)]
        public string Metodo_Pago { get; set; }

        [MaxLength(100)]
        public string? Detalles_Tarjeta { get; set; }

        [Required, MaxLength(20)]
        public string Estado { get; set; }

        // fk y objeto de relacion para pedido
        public int ID_Pedido { get; set; }
        public Pedido Pedido { get; set; }
    }
}