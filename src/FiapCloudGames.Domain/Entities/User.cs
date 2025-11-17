using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FiapCloudGames.Domain.Enums;

namespace FiapCloudGames.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]

        public required string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "O papel do usuário é obrigatório.")]
        public required UserRole Role { get; set; } = UserRole.User;

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Library> LibraryGames { get; set; } = new List<Library>();

        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(PasswordHash))
            {
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        public void SetPassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}

