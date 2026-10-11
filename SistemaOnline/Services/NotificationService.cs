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
        // Claves de notificaciones de "un solo evento" ya generadas (ver AgregarUnaVez),
        // para no duplicarlas en cada sondeo/recarga aunque ya se hayan marcado como leídas.
        private static readonly HashSet<string> _clavesGeneradas = new();
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

        // Igual que Agregar, pero solo crea la notificación la primera vez que se use
        // esta "clave". Pensado para notificaciones derivadas de un estado (p.ej. "pedido
        // #N está listo") que se intentan generar repetidamente desde varios puntos
        // (sondeo periódico, recarga de página, la propia transición de estado): evita que
        // dos o más pedidos lleguen a competir por una sola notificación o que una mesa con
        // pedido listo se quede sin la suya si el evento puntual se perdió por cualquier motivo.
        public static void AgregarUnaVez(string clave, string icono, string titulo, string detalle)
        {
            lock (_lock)
            {
                if (!_clavesGeneradas.Add(clave)) return;
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