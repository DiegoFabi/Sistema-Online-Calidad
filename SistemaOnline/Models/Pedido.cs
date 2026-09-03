using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Pedido
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Pedido { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required, MaxLength(50)]
        public string Estado_Pedido { get; set; }

        [MaxLength(500)]
        public string? Detalle_Pedido { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El subtotal no puede ser negativo.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El total no puede ser negativo.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        // fk y objeto de relacion para empleado
        public int ID_Empleado { get; set; }
        public Empleado Empleado { get; set; }

        // fk y objeto de relacion para mesa_restaurante
        public int ID_Mesa { get; set; }
        public Mesa_Restaurante Mesa_Restaurante { get; set; }

        // colecciones dependientes del pedido
        public ICollection<Pago> Pagos { get; set; }
        public ICollection<Comprobante_Pago> Comprobantes_Pago { get; set; }
        public ICollection<Pedido_Detalle> Pedido_Detalles { get; set; }
    }
}