using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByCodeAsync(int code);
        Task<User> CreateUserAsync(RegisterDto user);
    }
}
