namespace SistemaOnline.Services
{
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
    }
}