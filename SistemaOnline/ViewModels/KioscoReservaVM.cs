using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class KioscoReservaVM
    {
        public int ID_Mesa { get; set; }

        public int Numero_Mesa { get; set; }

        [Required(ErrorMessage = "La fecha y hora de la reservación son obligatorias.")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha_Hora { get; set; }

        [Range(1, 50, ErrorMessage = "El número de personas debe ser entre 1 y 50.")]
        public int Numero_Personas { get; set; }

        [Required, MaxLength(100)]
        public string Ocasion_Especial { get; set; } = "Ninguna";

        [MaxLength(300)]
        public string? Notas { get; set; }
    }
}