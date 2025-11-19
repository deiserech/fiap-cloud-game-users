using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Domain.Interfaces.Repositories
{
    public interface IGameRepository
    {
        Task<Game?> GetByIdAsync(Guid id);
        Task<Game> CreateAsync(Game game);
        Task<Game> UpdateAsync(Game game);
        Task RemoveAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
