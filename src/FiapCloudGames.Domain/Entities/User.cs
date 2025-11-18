using FiapCloudGames.Domain.Enums;

namespace FiapCloudGames.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }

        public required string Email { get; set; }

        public required UserRole Role { get; set; } = UserRole.User;

        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<Library> LibraryGames { get; set; } = [];

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

