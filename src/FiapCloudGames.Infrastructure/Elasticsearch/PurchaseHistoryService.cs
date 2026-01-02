using System;
using System.Threading;
using System.Threading.Tasks;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Infrastructure.Elasticsearch;
using Microsoft.Extensions.Logging;
using Nest;

namespace FiapCloudGames.Users.Infrastructure.Elasticsearch
{
    public class PurchaseHistoryService : IPurchaseHistoryService
    {
        private readonly IElasticClient _client;
        private readonly ILogger<PurchaseHistoryService> _logger;
        private const string IndexName = "purchases-history";

        public PurchaseHistoryService(IElasticClient client, ILogger<PurchaseHistoryService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task IndexPurchaseAsync(EnrichedPurchaseDto purchase, CancellationToken cancellationToken = default)
        {
            try
            {
                var doc = new PurchaseHistoryDocument
                {
                    PurchaseId = purchase.PurchaseId,
                    UserCode = purchase.UserCode,
                    UserId = purchase.UserId,
                    GameCode = purchase.GameCode,
                    GameId = purchase.GameId,
                    ProcessedAt = purchase.ProcessedAt,
                    Success = purchase.Success,
                    Amount = purchase.Amount,
                    Category = purchase.Category
                };

                var resp = await _client.IndexAsync(doc, i => i.Index(IndexName), cancellationToken);
                if (!resp.IsValid)
                {
                    _logger.LogWarning("Failed to index purchase history: {Reason}", resp.OriginalException?.Message ?? resp.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception indexing purchase history");
            }
        }
    }
}
