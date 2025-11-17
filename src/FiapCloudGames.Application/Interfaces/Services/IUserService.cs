using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);
        Task<User> CreateUserAsync(RegisterDto user);
        Task<bool> ExistsAsync(int id);
    }
}
