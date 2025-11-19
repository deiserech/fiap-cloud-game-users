using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User> CreateUserAsync(RegisterDto user);
        Task<bool> ExistsAsync(Guid id);
    }
}
