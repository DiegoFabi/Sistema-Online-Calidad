using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class RegistroVM
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string Nombre_Usuario { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
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