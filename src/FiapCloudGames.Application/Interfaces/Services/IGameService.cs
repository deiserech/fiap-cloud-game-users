using FiapCloudGames.Application.DTOs;

namespace FiapCloudGames.Application.Interfaces.Services
{
    public interface IGameService
    {
        Task<IEnumerable<GameDto>> GetAllAsync();
        Task<GameDto?> GetByIdAsync(int id);
        Task<GameDto> CreateAsync(GameDto gameDto);
    }
}
