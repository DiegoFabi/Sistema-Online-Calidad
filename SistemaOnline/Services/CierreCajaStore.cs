using System.Security.Claims;

namespace SistemaOnline.Services
{
    public class RegistroCierreCaja
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime HoraCierre { get; set; } = DateTime.Now;
        public string Usuario { get; set; } = "Sistema";
        public decimal EfectivoEsperado { get; set; }
        public decimal EfectivoContado { get; set; }
        public decimal Diferencia => EfectivoContado - EfectivoEsperado;
        public decimal TotalGeneral { get; set; }
    }

    // Cierre de turno del Cajero: efectivo esperado vs. contado, con la hora del cierre.
    // En memoria, igual que NotificacionStore/LoginAttemptStore/AuditoriaStore: no requiere
    // ninguna tabla ni columna nueva en la base de datos. Se reinicia si la app se reinicia.
    public static class CierreCajaStore
    {
        private static readonly List<RegistroCierreCaja> _cierres = new();
        private static int _nextId = 1;
        private static readonly object _lock = new();

        public static RegistroCierreCaja Registrar(ClaimsPrincipal? usuarioActual, DateTime fecha, decimal efectivoEsperado, decimal efectivoContado, decimal totalGeneral)
        {
            string nombre = usuarioActual?.Identity?.Name ?? "Sistema";

            lock (_lock)
            {
                var registro = new RegistroCierreCaja
                {
                    Id = _nextId++,
                    Fecha = fecha.Date,
                    Usuario = nombre,
                    EfectivoEsperado = efectivoEsperado,
                    EfectivoContado = efectivoContado,
                    TotalGeneral = totalGeneral
                };
                _cierres.Add(registro);
                return registro;
            }
        }

        public static RegistroCierreCaja? ObtenerPorFecha(DateTime fecha)
        {
            lock (_lock)
            {
                return _cierres
                    .Where(c => c.Fecha == fecha.Date)
                    .OrderByDescending(c => c.HoraCierre)
                    .FirstOrDefault();
            }
        }
    }
}