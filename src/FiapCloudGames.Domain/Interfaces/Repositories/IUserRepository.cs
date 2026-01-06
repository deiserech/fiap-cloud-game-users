using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailAndCodeAsync(string email, int code);
        Task<User> CreateAsync(User user);
        Task<User?> GetByCodeAsync(int code);
        Task<bool> EmailExistsAsync(string email);
        Task<IReadOnlyCollection<User>> GetAllAsync();
    }
}
