using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class CartaVM
    {
        public int ID_Carta { get; set; }

        [Required, MaxLength(100)]
        public string Nombre_Carta { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad de platos no puede ser negativa.")]
        public int Cantidad_Platos { get; set; }

        [Required, MaxLength(100)]
        public string Descripcion { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }
    }
}
