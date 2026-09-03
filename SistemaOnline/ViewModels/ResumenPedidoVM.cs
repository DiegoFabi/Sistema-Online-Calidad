namespace SistemaOnline.ViewModels
{
    public class ResumenPedidoVM
    {
        public int ID_Pedido { get; set; }
        public string MesaNumero { get; set; } = "-";
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public List<ResumenItemVM> Items { get; set; } = new();
    }

    public class ResumenItemVM
    {
        public string Nombre { get; set; } = "-";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}