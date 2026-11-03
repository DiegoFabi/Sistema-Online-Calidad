namespace SistemaOnline.Services
{
    // Logo personalizable del sistema. El archivo se guarda en disco (no en la base de datos):
    // por defecto en App_Data/branding junto a la app, o en la carpeta indicada por la clave
    // Branding:Carpeta (variable de entorno Branding__Carpeta), útil para apuntar a un volumen
    // persistente en Docker o Railway. Sin archivo, las vistas muestran el icono "sin logo".
    public class LogoService
    {
        public const long TamanoMaximoBytes = 2 * 1024 * 1024;

        private const string NombreBase = "logo";
        private static readonly Dictionary<string, string> TiposPorExtension = new()
        {
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".webp"] = "image/webp"
        };

        private readonly string _carpeta;
        private readonly object _bloqueo = new();

        public LogoService(IConfiguration configuracion)
        {
            var carpeta = configuracion["Branding:Carpeta"];
            _carpeta = string.IsNullOrWhiteSpace(carpeta)
                ? Path.Combine(AppContext.BaseDirectory, "App_Data", "branding")
                : carpeta;
        }

        // Ruta del logo actual, o null si no se ha subido ninguno.
        public string? RutaActual()
        {
            if (!Directory.Exists(_carpeta)) return null;
            foreach (var ext in TiposPorExtension.Keys)
            {
                var ruta = Path.Combine(_carpeta, NombreBase + ext);
                if (File.Exists(ruta)) return ruta;
            }
            return null;
        }

        public bool TieneLogo => RutaActual() != null;

        // Cambia cada vez que se reemplaza el logo: sirve para que el navegador no use una copia vieja.
        public long Version
        {
            get
            {
                var ruta = RutaActual();
                return ruta == null ? 0 : File.GetLastWriteTimeUtc(ruta).Ticks;
            }
        }

        public static string TipoContenido(string ruta) =>
            TiposPorExtension.TryGetValue(Path.GetExtension(ruta).ToLowerInvariant(), out var tipo) ? tipo : "application/octet-stream";

        // Valida y guarda el logo. Devuelve un mensaje de error, o null si se guardó.
        public async Task<string?> GuardarAsync(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return "Selecciona una imagen para el logo.";
            if (archivo.Length > TamanoMaximoBytes)
                return "El logo no puede pesar más de 2 MB.";

            // El tipo se decide por el contenido real del archivo, no por su nombre ni por lo que
            // declara el navegador, para que no se pueda subir otra cosa con extensión de imagen.
            var cabecera = new byte[12];
            await using (var lectura = archivo.OpenReadStream())
            {
                int leidos = await lectura.ReadAsync(cabecera);
                if (leidos < cabecera.Length) return "El archivo no es una imagen válida.";
            }
            var extension = DetectarExtension(cabecera);
            if (extension == null)
                return "Formato no admitido: usa una imagen PNG, JPG o WEBP.";

            Directory.CreateDirectory(_carpeta);
            var destino = Path.Combine(_carpeta, NombreBase + extension);
            var temporal = destino + ".tmp";
            await using (var escritura = File.Create(temporal))
            {
                await archivo.CopyToAsync(escritura);
            }
            lock (_bloqueo)
            {
                EliminarArchivos();
                File.Move(temporal, destino);
            }
            return null;
        }

        public void Eliminar()
        {
            lock (_bloqueo)
            {
                EliminarArchivos();
            }
        }

        private void EliminarArchivos()
        {
            if (!Directory.Exists(_carpeta)) return;
            foreach (var ext in TiposPorExtension.Keys)
            {
                var ruta = Path.Combine(_carpeta, NombreBase + ext);
                if (File.Exists(ruta)) File.Delete(ruta);
            }
        }

        private static string? DetectarExtension(byte[] c)
        {
            if (c[0] == 0x89 && c[1] == 0x50 && c[2] == 0x4E && c[3] == 0x47) return ".png";
            if (c[0] == 0xFF && c[1] == 0xD8 && c[2] == 0xFF) return ".jpg";
            if (c[0] == 0x52 && c[1] == 0x49 && c[2] == 0x46 && c[3] == 0x46 &&
                c[8] == 0x57 && c[9] == 0x45 && c[10] == 0x42 && c[11] == 0x50) return ".webp";
            return null;
        }
    }
}