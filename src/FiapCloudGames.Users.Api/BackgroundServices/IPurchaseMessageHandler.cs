using FiapCloudGames.Users.Domain.Events;
using System.Threading;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public interface IPurchaseMessageHandler
    {
        Task HandleAsync(PurchaseCompletedEvent message, CancellationToken cancellationToken);
    }
}
