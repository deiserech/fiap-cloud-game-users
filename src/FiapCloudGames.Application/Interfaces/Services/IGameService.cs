using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Domain.Entities.Events;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface IGameService
    {
        Task ProcessAsync(GameEvent message, CancellationToken cancellationToken = default);
    }
}
