namespace SistemaOnline.Models
{
    public class Producto_Promocion
    {
        public int ID_Producto { get; set; }
        public Producto Producto { get; set; }

        public int ID_Promocion { get; set; }
        public Promocion Promocion { get; set; }
    }
}