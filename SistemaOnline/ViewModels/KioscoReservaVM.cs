using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    // ViewModel para la reservacion que un Cliente autenticado hace desde el Kiosco:
    // a diferencia de ReservacionVM (uso interno del Administrador/Mesero), aqui no se
    // elige Cliente ni Mesa por un select; el Cliente es el usuario que inicio sesion
    // y la Mesa viene fija del boton "Reservar" sobre el que hizo clic.
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
