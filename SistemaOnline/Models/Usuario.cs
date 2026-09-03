using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Usuario { get; set; }

        [Required, MaxLength(50)]
        public string Nombre_Usuario { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }

        public bool Estado { get; set; }

        // Relacion con Rol
        public int ID_Rol { get; set; }
        public Rol Rol { get; set; }
    }
}
