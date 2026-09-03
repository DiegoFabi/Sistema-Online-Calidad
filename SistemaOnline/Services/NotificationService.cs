namespace SistemaOnline.Services
{
    public class Notificacion
    {
        public int Id { get; set; }
        public string Icono { get; set; } = "notifications";
        public string Titulo { get; set; } = "";
        public string Detalle { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        public bool Leida { get; set; } = false;
    }

    public static class NotificacionStore
    {
        private static readonly List<Notificacion> _notificaciones = new();
        private static int _nextId = 1;
        private static readonly object _lock = new();

        public static void Agregar(string icono, string titulo, string detalle)
        {
            lock (_lock)
            {
                _notificaciones.Insert(0, new Notificacion
                {
                    Id = _nextId++,
                    Icono = icono,
                    Titulo = titulo,
                    Detalle = detalle
                });
            }
        }

        public static List<Notificacion> ObtenerNoLeidas()
        {
            lock (_lock)
            {
                return _notificaciones.Where(n => !n.Leida).ToList();
            }
        }

        public static void MarcarLeida(int id)
        {
            lock (_lock)
            {
                var n = _notificaciones.FirstOrDefault(x => x.Id == id);
                if (n != null) n.Leida = true;
            }
        }
    }
}