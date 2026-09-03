using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class Mesa_RestauranteVM
    {
        public int ID_Mesa { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El número de mesa debe ser mayor a 0.")]
        public int Numero_Mesa { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor a 0.")]
        public int Capacidad { get; set; }

        [Required, MaxLength(100)]
        public string Ubicacion { get; set; }

        [Required, MaxLength(20)]
        public string Estado { get; set; }
    }
}
