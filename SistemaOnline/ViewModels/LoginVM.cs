using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class LoginVM
    {
        public string? Username { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MaxLength(255)]
        public string Password { get; set; }

        public string? RepeatPassword { get; set; }
    }
}
