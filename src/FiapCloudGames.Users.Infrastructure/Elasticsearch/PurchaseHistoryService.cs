using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
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
                await EnsureIndexExistsAsync(cancellationToken);

                var doc = new PurchaseHistoryDocument
                {
                    PurchaseId = purchase.PurchaseId,
                    UserCode = purchase.UserCode,
                    UserId = purchase.UserId,
                    GameCode = purchase.GameCode,
                    GameId = purchase.GameId,
                    ProcessedAt = purchase.ProcessedAt,
                    GameTitle = purchase.GameTitle,
                    Category = purchase.Category
                };

                var resp = await _client.IndexAsync(doc, i => i.Index(IndexName), cancellationToken);
                if (!resp.IsValid)
                {
                    _logger.LogWarning("Failed to index purchase history: {Reason}", resp.OriginalException?.Message ?? resp.DebugInformation);
                    return;
                }
                _logger.LogDebug("Succefully indexed purchase history {@Purchase}", doc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception indexing purchase history");
            }
        }
        protected virtual async Task EnsureIndexExistsAsync(CancellationToken cancellationToken = default)
        {
            var existsResponse = await _client.Indices.ExistsAsync(IndexName, ct: cancellationToken);
            if (!existsResponse.Exists)
            {
                var createResponse = await _client.Indices.CreateAsync(IndexName, c => c
                    .Map<PurchaseHistoryDocument>(m => m
                        .AutoMap()
                        .Properties(ps => ps
                            .Keyword(k => k.Name(n => n.PurchaseId))
                            .Keyword(k => k.Name(n => n.UserId))
                            .Keyword(k => k.Name(n => n.GameId))
                            .Keyword(k => k.Name(n => n.Category))
                            .Text(nu => nu.Name(n => n.GameTitle))
                            .Number(nu => nu.Name(n => n.UserCode).Type(NumberType.Integer))
                            .Number(nu => nu.Name(n => n.GameCode).Type(NumberType.Integer))
                            .Date(d => d.Name(n => n.ProcessedAt))
                        )
                    ), ct: cancellationToken
                );

                if (!createResponse.IsValid)
                {
                    _logger.LogError("Failed to create purchases-history index: {Reason}", createResponse.OriginalException?.Message ?? createResponse.DebugInformation);
                }
                else
                {
                    _logger.LogInformation("Created purchases-history index with explicit mapping.");
                }
            }
        }

    }
}
