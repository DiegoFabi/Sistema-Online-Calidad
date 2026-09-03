using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class PromocionVM
    {
        public int ID_Promocion { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required, MaxLength(300)]
        public string Descripcion { get; set; }

        [Range(0.01, 100, ErrorMessage = "El porcentaje de descuento debe ser entre 0.01% y 100%.")]
        public decimal Porcentaje_Descuento { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Fecha_Inicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Fecha_Fin { get; set; }

        public bool Estado { get; set; }
    }
}
