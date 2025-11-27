using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Entities.Events;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface IGameService
    {
        Task ProcessAsync(GameEvent message, CancellationToken cancellationToken = default);
        Task<Game?> GetByCodeAsync(int code);
    }
}
