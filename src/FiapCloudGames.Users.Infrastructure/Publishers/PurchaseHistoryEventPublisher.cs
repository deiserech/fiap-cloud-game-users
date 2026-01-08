using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Publishers;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Infrastructure.Publishers
{
    public class PurchaseHistoryEventPublisher : IPurchaseHistoryEventPublisher
    {
        private readonly IServiceBusPublisher _busPublisher;
        private readonly ILogger<PurchaseHistoryEventPublisher> _logger;
        private const string PurchaseHistoryTopic = "purchases-history-index";

        public PurchaseHistoryEventPublisher(IServiceBusPublisher busPublisher, ILogger<PurchaseHistoryEventPublisher> logger)
        {
            _busPublisher = busPublisher;
            _logger = logger;
        }

        public async Task PublishPurchaseHistoryAsync(EnrichedPurchaseDto purchase, CancellationToken cancellationToken = default)
        {
            try
            {
                await _busPublisher.PublishAsync(purchase, PurchaseHistoryTopic);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao publicar evento de histórico de compra {PurchaseId}", purchase.PurchaseId);
            }
        }
    }
}
