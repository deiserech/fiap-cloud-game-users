using FiapCloudGames.Users.Application.DTOs;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface IPurchaseHistoryService
    {
        Task IndexPurchaseAsync(EnrichedPurchaseDto purchase, CancellationToken cancellationToken = default);
    }
}
