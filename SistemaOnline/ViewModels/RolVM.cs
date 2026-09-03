using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class RolVM
    {
        public int ID_Rol { get; set; }

        [Required, MaxLength(50)]
        public string Nombre_Rol { get; set; }
    }
}
