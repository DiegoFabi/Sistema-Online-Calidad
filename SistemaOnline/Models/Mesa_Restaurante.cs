using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Mesa_Restaurante
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Mesa { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El número de mesa debe ser mayor a 0.")]
        public int Numero_Mesa { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor a 0.")]
        public int Capacidad { get; set; }

        [Required, MaxLength(100)]
        public string Ubicacion { get; set; }

        [Required, MaxLength(20)]
        public string Estado { get; set; }

        // una mesa recibe muchos pedidos y reservaciones
        public ICollection<Pedido> Pedidos { get; set; }
        public ICollection<Reservacion> Reservaciones { get; set; }
    }
}