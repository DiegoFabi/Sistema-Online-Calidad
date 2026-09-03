namespace SistemaOnline.Models
{
    public class Proveedor_Ingrediente
    {
        // las llaves compuestas se declaran en el appdbcontext fluent api
        public int ID_Proveedor { get; set; }
        public Proveedor Proveedor { get; set; }

        public int ID_Ingrediente { get; set; }
        public Ingrediente Ingrediente { get; set; }
    }
}