using FiapCloudGames.Users.Domain.Entities.Events;
using System.Threading;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public interface IGameMessageHandler
    {
        Task HandleAsync(GameEvent message, CancellationToken cancellationToken);
    }
}
