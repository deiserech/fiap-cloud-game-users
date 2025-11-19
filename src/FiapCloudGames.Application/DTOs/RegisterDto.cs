using System.ComponentModel.DataAnnotations;
using FiapCloudGames.Users.Domain.Enums;

namespace FiapCloudGames.Users.Application.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [StringLength(255, ErrorMessage = "O e-mail deve ter no máximo 255 caracteres.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 128 caracteres.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
        public required UserRole Role { get; set; }
    }
}
