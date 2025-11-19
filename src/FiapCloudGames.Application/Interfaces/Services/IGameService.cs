using FiapCloudGames.Users.Application.DTOs;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface IGameService
    {
        Task<GameDto?> GetByIdAsync(Guid id);
        Task<GameDto> CreateAsync(GameDto gameDto);
    }
}
