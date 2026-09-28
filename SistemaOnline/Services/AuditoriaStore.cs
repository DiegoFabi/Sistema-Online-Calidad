using System.Security.Claims;

namespace SistemaOnline.Services
{
    public class RegistroAuditoria
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Usuario { get; set; } = "Sistema";
        public string Rol { get; set; } = "-";
        public string Accion { get; set; } = "";
        public string Entidad { get; set; } = "";
        public string Detalle { get; set; } = "";
    }

    public static class AuditoriaStore
    {
        private const int MaxRegistros = 500;
        private static readonly List<RegistroAuditoria> _registros = new();
        private static int _nextId = 1;
        private static readonly object _lock = new();

        public static void Registrar(ClaimsPrincipal? usuarioActual, string accion, string entidad, string detalle)
        {
            string nombre = usuarioActual?.Identity?.Name ?? "Sistema";
            string rol = usuarioActual?.FindFirst(ClaimTypes.Role)?.Value ?? "-";

            lock (_lock)
            {
                _registros.Insert(0, new RegistroAuditoria
                {
                    Id = _nextId++,
                    Usuario = nombre,
                    Rol = rol,
                    Accion = accion,
                    Entidad = entidad,
                    Detalle = detalle
                });

                if (_registros.Count > MaxRegistros)
                {
                    _registros.RemoveAt(_registros.Count - 1);
                }
            }
        }

        public static List<RegistroAuditoria> Obtener(string? entidad = null)
        {
            lock (_lock)
            {
                IEnumerable<RegistroAuditoria> query = _registros;
                if (!string.IsNullOrWhiteSpace(entidad))
                {
                    query = query.Where(r => r.Entidad.Equals(entidad, StringComparison.OrdinalIgnoreCase));
                }
                return query.ToList();
            }
        }

        public static List<string> ObtenerEntidades()
        {
            lock (_lock)
            {
                return _registros.Select(r => r.Entidad).Distinct().OrderBy(e => e).ToList();
            }
        }
    }
}