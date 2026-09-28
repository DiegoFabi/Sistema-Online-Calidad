using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class RestablecerPasswordVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(255, ErrorMessage = "La contraseña no puede superar los 255 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
            ErrorMessage = "La contraseña debe incluir al menos una letra y un número.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debes repetir la contraseña.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string RepeatPassword { get; set; }
    }
}