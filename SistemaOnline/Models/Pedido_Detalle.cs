using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Pedido_Detalle
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Pedido_Detalle { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        // fk y objeto de relacion para pedido
        public int ID_Pedido { get; set; }
        public Pedido Pedido { get; set; }

        // fk y objeto de relacion para producto
        public int ID_Producto { get; set; }
        public Producto Producto { get; set; }
    }
}