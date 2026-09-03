using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Reservacion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Reservacion { get; set; }

        public DateTime Fecha_Hora { get; set; }

        public int Numero_Personas { get; set; }

        [Required, MaxLength(100)]
        public string Ocasion_Especial { get; set; }

        [Required, MaxLength(20)]
        public string Estado_Reservacion { get; set; }

        [Required, MaxLength(300)]
        public string Notas { get; set; }

        // fk y objeto de relacion para cliente
        public int ID_Cliente { get; set; }
        public Cliente Cliente { get; set; }

        // fk y objeto de relacion para mesa_restaurante
        public int ID_Mesa { get; set; }
        public Mesa_Restaurante Mesa_Restaurante { get; set; }
    }
}