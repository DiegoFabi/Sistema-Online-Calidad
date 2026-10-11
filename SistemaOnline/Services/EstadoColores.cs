namespace SistemaOnline.Services
{
    // Mapeo de Estado -> clases de color, compartido por todas las vistas que muestran
    // un estado de Reservacion (Dashboard, Administrador/Reservaciones, etc.) para que
    // el mismo estado se vea siempre con el mismo color en todo el sistema.
    public static class EstadoColores
    {
        public static string Reservacion(string estado) => estado switch
        {
            "Pendiente" => "bg-caution-yellow text-[#5b4300]",
            "Confirmada" => "bg-success-green text-white",
            "Completada" => "bg-success-green text-white",
            "Cancelada" => "bg-alert-red text-white",
            _ => "bg-surface-container-high text-on-surface-variant"
        };

        // Estado del stock de un Inventario. No es un campo del modelo: se calcula al vuelo
        // comparando Cantidad_Stock contra los umbrales Stock_Minimo/Stock_Maximo que ya
        // tiene el propio registro, puramente para mostrarlo en Inventario/Lista.
        public static (string Texto, string Clases) InventarioEstado(decimal cantidadStock, decimal stockMinimo, decimal stockMaximo)
        {
            if (cantidadStock <= stockMinimo)
                return ("Crítico", "bg-alert-red text-white");
            if (cantidadStock >= stockMaximo)
                return ("Completo", "bg-success-green text-white");
            return ("Normal", "bg-caution-yellow text-[#5b4300]");
        }

        // Color + ícono por Estado_Pedido, para la insignia que se repite en Dashboard,
        // Administrador, Cajero, Cocina y Mesero. Antes cada vista tenía su propia variante
        // (algunas con un solo color para todos los estados, otras con switches parciales e
        // incluso contradictorios entre sí para el mismo estado) — ver _EstadoPedidoBadge.cshtml,
        // el partial que usa este método y que centraliza también el marcado de la insignia.
        public static (string Clases, string Icono) Pedido(string estado) => estado switch
        {
            "Pendiente" => ("bg-error-container text-on-error-container", "schedule"),
            "En Cocina" => ("bg-tertiary-container text-on-tertiary-container", "soup_kitchen"),
            "Preparando" => ("bg-primary-container text-on-primary-container", "local_fire_department"),
            "Listo" => ("bg-secondary-container text-on-secondary-container", "notifications_active"),
            "Entregado" => ("bg-[#0d6efd] text-white", "room_service"),
            "Servido" => ("bg-[#0d6efd] text-white", "room_service"),
            "Pagado" => ("bg-success-green text-white", "paid"),
            "Cancelado" => ("bg-alert-red text-white", "cancel"),
            _ => ("bg-surface-container-high text-on-surface-variant", "help")
        };
    }
}