using FiapCloudGames.Users.Domain.Events;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface IPurchaseService
    {
        Task ProcessAsync(PurchaseCompletedEvent message, CancellationToken cancellationToken);
    }
}