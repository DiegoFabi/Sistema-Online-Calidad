using System.ComponentModel.DataAnnotations;

namespace SistemaOnline.ViewModels
{
    public class RecuperarPasswordVM
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
        public string Email { get; set; }
    }
}