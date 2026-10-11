using System.Text.Json;

namespace SistemaOnline.Services
{
    public static class LoginAttemptStore
    {
        private const int MaxIntentos = 5;
        private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);
        private static readonly string RutaArchivo = Path.Combine(AppContext.BaseDirectory, "App_Data", "login_lockouts.json");

        private class Estado
        {
            public int Intentos { get; set; }
            public DateTime? BloqueadoHasta { get; set; }
        }

        private static readonly Dictionary<string, Estado> _intentos = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new();
        private static bool _cargado = false;

        // Carga el archivo de respaldo una sola vez, la primera vez que se usa el store
        // tras arrancar el proceso. Si el archivo no existe o esta corrupto, se continua
        // con el diccionario vacio (igual que el comportamiento original en memoria).
        private static void AsegurarCargado()
        {
            if (_cargado) return;
            try
            {
                if (File.Exists(RutaArchivo))
                {
                    var json = File.ReadAllText(RutaArchivo);
                    var datos = JsonSerializer.Deserialize<Dictionary<string, Estado>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (datos != null)
                    {
                        foreach (var kv in datos)
                        {
                            _intentos[kv.Key] = kv.Value;
                        }
                    }
                }
            }
            catch
            {
                // Archivo ausente, ilegible o con formato antiguo: se continua sin bloqueos
                // previos en vez de impedir el arranque de la aplicacion.
            }
            finally
            {
                _cargado = true;
            }
        }

        private static void Guardar()
        {
            try
            {
                var carpeta = Path.GetDirectoryName(RutaArchivo)!;
                Directory.CreateDirectory(carpeta);
                var json = JsonSerializer.Serialize(_intentos);
                File.WriteAllText(RutaArchivo, json);
            }
            catch
            {
                // Si no se puede escribir a disco (permisos, disco lleno, etc.) el bloqueo
                // sigue funcionando en memoria para el proceso actual; solo se pierde la
                // persistencia entre reinicios.
            }
        }

        public static (bool bloqueado, int minutosRestantes) EstaBloqueado(string email)
        {
            lock (_lock)
            {
                AsegurarCargado();
                if (_intentos.TryGetValue(email, out var estado) && estado.BloqueadoHasta.HasValue)
                {
                    if (estado.BloqueadoHasta.Value > DateTime.Now)
                    {
                        int minutos = (int)Math.Ceiling((estado.BloqueadoHasta.Value - DateTime.Now).TotalMinutes);
                        return (true, Math.Max(minutos, 1));
                    }
                    // el bloqueo ya expiro: se reinicia el contador
                    _intentos.Remove(email);
                    Guardar();
                }
                return (false, 0);
            }
        }

        // Registra un intento fallido y devuelve cuantos intentos quedan antes del bloqueo.
        public static int RegistrarFallo(string email, out bool quedoBloqueado)
        {
            lock (_lock)
            {
                AsegurarCargado();
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
                    Guardar();
                    return 0;
                }

                quedoBloqueado = false;
                Guardar();
                return MaxIntentos - estado.Intentos;
            }
        }

        public static void RegistrarExito(string email)
        {
            lock (_lock)
            {
                AsegurarCargado();
                if (_intentos.Remove(email))
                {
                    Guardar();
                }
            }
        }
    }
}