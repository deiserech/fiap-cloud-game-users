using FiapCloudGames.Users.Application.DTOs;

namespace FiapCloudGames.Users.Application.Interfaces.Publishers
{
    public interface IPurchaseHistoryEventPublisher
    {
        Task PublishPurchaseHistoryAsync(EnrichedPurchaseDto purchase, CancellationToken cancellationToken = default);
    }
}
