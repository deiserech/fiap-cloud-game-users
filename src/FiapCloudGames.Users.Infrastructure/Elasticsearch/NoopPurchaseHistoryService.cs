using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Infrastructure.Elasticsearch
{
    public class NoopPurchaseHistoryService : IPurchaseHistoryService
    {
        private readonly ILogger<NoopPurchaseHistoryService> _logger;

        public NoopPurchaseHistoryService(ILogger<NoopPurchaseHistoryService> logger)
        {
            _logger = logger;
        }

        public Task IndexPurchaseAsync(EnrichedPurchaseDto purchase, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Elasticsearch disabled. Skipping purchase history index. PurchaseId={PurchaseId}", purchase.PurchaseId);
            return Task.CompletedTask;
        }
    }
}
