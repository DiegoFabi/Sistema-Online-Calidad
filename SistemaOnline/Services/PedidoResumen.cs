using SistemaOnline.Models;

namespace SistemaOnline.Services
{
    // Texto de mesa / resumen de un Pedido, compartido por las vistas y listas que
    // necesitan mostrar a qué mesa pertenece un pedido (Cajero/Pagos, Pago/Lista,
    // Comprobante_Pago/Lista...), para no repetir el mismo ternario "Mesa X" / "-"
    // en cada controlador o vista.
    public static class PedidoResumen
    {
        public static string Mesa(Pedido? pedido) =>
            pedido?.Mesa_Restaurante != null ? "Mesa " + pedido.Mesa_Restaurante.Numero_Mesa : "-";

        // "Pedido #49 - Mesa 1 - Entregado": texto usado en la columna "Pedido" de
        // Pago/Lista y Comprobante_Pago/Lista.
        public static string Resumen(Pedido pedido) =>
            $"Pedido #{pedido.ID_Pedido} - {Mesa(pedido)} - {pedido.Estado_Pedido}";
    }
}
