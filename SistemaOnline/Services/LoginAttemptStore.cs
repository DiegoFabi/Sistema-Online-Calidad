namespace SistemaOnline.Services
{
    // Bloqueo temporal por intentos fallidos de inicio de sesion. En memoria,
    // igual que NotificacionStore: no requiere ninguna tabla ni columna nueva
    // en la base de datos. Se reinicia si la aplicacion se reinicia.
    public static class LoginAttemptStore
    {
        private const int MaxIntentos = 5;
        private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

        private class Estado
        {
            public int Intentos;
            public DateTime? BloqueadoHasta;
        }

        private static readonly Dictionary<string, Estado> _intentos = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new();

        public static (bool bloqueado, int minutosRestantes) EstaBloqueado(string email)
        {
            lock (_lock)
            {
                if (_intentos.TryGetValue(email, out var estado) && estado.BloqueadoHasta.HasValue)
                {
                    if (estado.BloqueadoHasta.Value > DateTime.Now)
                    {
                        int minutos = (int)Math.Ceiling((estado.BloqueadoHasta.Value - DateTime.Now).TotalMinutes);
                        return (true, Math.Max(minutos, 1));
                    }
                    // el bloqueo ya expiro: se reinicia el contador
                    _intentos.Remove(email);
                }
                return (false, 0);
            }
        }

        // Registra un intento fallido y devuelve cuantos intentos quedan antes del bloqueo.
        public static int RegistrarFallo(string email, out bool quedoBloqueado)
        {
            lock (_lock)
            {
                if (!_intentos.TryGetValue(email, out var estado))
                {
                    estado = new Estado();
                    _intentos[email] = estado;
                }

                estado.Intentos++;
                if (estado.Intentos >= MaxIntentos)
                {
                    estado.BloqueadoHasta = DateTime.Now.Add(DuracionBloqueo);
                    quedoBloqueado = true;
                    return 0;
                }

                quedoBloqueado = false;
                return MaxIntentos - estado.Intentos;
            }
        }

        public static void RegistrarExito(string email)
        {
            lock (_lock)
            {
                _intentos.Remove(email);
            }
        }
    }
}