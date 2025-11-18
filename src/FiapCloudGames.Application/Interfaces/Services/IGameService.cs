using FiapCloudGames.Application.DTOs;

namespace FiapCloudGames.Application.Interfaces.Services
{
    public interface IGameService
    {
        Task<GameDto?> GetByIdAsync(Guid id);
        Task<GameDto> CreateAsync(GameDto gameDto);
    }
}
