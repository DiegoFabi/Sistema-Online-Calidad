using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaOnline.Models
{
    public class Rol
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_Rol { get; set; }

        [StringLength(50)]
        public string Nombre_Rol { get; set; }

        // Un rol tiene muchos usuarios
        public ICollection<Usuario> Usuarios { get; set; }
    }
}