using Azure.Messaging.ServiceBus;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public class PurchaseHistoryConsumer : BackgroundService
    {
        private readonly IServiceBusClientWrapper _sb;
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceBusProcessor? _processor;
        private readonly IConfiguration _config;
        private readonly ILogger<PurchaseHistoryConsumer> _logger;

        public PurchaseHistoryConsumer(
            IServiceBusClientWrapper sb,
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            ILogger<PurchaseHistoryConsumer> logger)
        {
            _sb = sb;
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _config["PURCHASE_HISTORY_TOPIC"] ?? "purchases-history-index";
            var subscription = _config["PURCHASE_HISTORY_SUBSCRIPTION"] ?? "fiap-cloud-games-users";

            _processor = _sb.CreateProcessorWrapper(topic, subscription);

            _processor.ProcessMessageAsync += async args =>
            {
                var body = args.Message.Body.ToString();
                var msg = JsonConvert.DeserializeObject<EnrichedPurchaseDto>(body);

                if (msg == null)
                {
                    _logger.LogWarning("PurchaseHistoryConsumer: mensagem inválida");
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var historyService = scope.ServiceProvider.GetRequiredService<IPurchaseHistoryService>();

                await historyService.IndexPurchaseAsync(msg, args.CancellationToken);

                await args.CompleteMessageAsync(args.Message);
            };

            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync(stoppingToken);
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            _logger.LogError($"PurchaseHistoryConsumer error: {arg.Exception}");
            return Task.CompletedTask;
        }
    }
}
