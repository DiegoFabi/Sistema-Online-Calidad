using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Empleado_Turno
    {
        // tabla intermedia para relacion muchos a muchos entre Empleado y Turno
        [Key, Column(Order = 0)]
        public int ID_Turno { get; set; }
        public Turno Turno { get; set; }

        [Key, Column(Order = 1)]
        public int ID_Empleado { get; set; }
        public Empleado Empleado { get; set; }
    }
}