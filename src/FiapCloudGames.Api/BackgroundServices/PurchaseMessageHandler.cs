using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Events;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public class PurchaseMessageHandler : IPurchaseMessageHandler
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<PurchaseMessageHandler> _logger;

        public PurchaseMessageHandler(IPurchaseService purchaseService, ILogger<PurchaseMessageHandler> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        public async Task HandleAsync(PurchaseCompletedEvent message, CancellationToken cancellationToken)
        {
            try
            {
                await _purchaseService.ProcessAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling purchase completed message");
                throw;
            }
        }
    }
}
