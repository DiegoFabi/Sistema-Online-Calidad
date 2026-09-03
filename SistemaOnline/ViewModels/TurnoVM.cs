using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class TurnoVM
    {
        public int ID_Turno { get; set; }

        [Required, MaxLength(50)]
        public string Nombre_Turno { get; set; }

        public TimeSpan Hora_Inicio { get; set; }

        public TimeSpan Hora_Fin { get; set; }

        [Required, MaxLength(15)]
        public string Dias_Semana { get; set; }
    }
}
