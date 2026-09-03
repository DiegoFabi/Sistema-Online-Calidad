using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaOnline.ViewModels
{
    public class UsuarioVM
    {
        public int ID_Usuario { get; set; }

        [Required, MaxLength(50)]
        public string Nombre_Usuario { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }
        [Required, MaxLength(255)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string RepeatPassword { get; set; }

        public bool Estado { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un rol válido.")]
        public int ID_Rol { get; set; }

        // Para mostrar en Lista
        public string? RolNombre { get; set; }

        // Para los selects en Nuevo/Editar
        public List<SelectListItem> RolesDisponibles { get; set; } = new();
    }
}
