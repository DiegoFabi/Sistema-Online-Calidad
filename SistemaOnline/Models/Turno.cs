using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Turno
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Turno { get; set; }

        [Required, MaxLength(50)]
        public string Nombre_Turno { get; set; }

        public TimeSpan Hora_Inicio { get; set; }

        public TimeSpan Hora_Fin { get; set; }

        [Required, MaxLength(15)]
        public string Dias_Semana { get; set; }

        // relacion muchos a muchos tabla intermedia
        public ICollection<Empleado_Turno> Empleado_Turnos { get; set; }
    }
}