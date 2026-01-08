using Azure.Messaging.ServiceBus;
using FiapCloudGames.Users.Domain.Events;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using FiapCloudGames.Users.Shared.Tracing;
using Newtonsoft.Json;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public class PurchaseCompletedConsumer : BackgroundService
    {
        private readonly IServiceBusClientWrapper _sb;
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceBusProcessor? _processor;
        private readonly IConfiguration _config;
        private readonly ILogger<PurchaseCompletedConsumer> _logger;

        public PurchaseCompletedConsumer(
            IServiceBusClientWrapper sb,
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            ILogger<PurchaseCompletedConsumer> logger)
        {
            _sb = sb;
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _config["PURCHASE_TOPIC"] ?? "payments-purchases-completed";
            var subscription = _config["PURCHASE_SUBSCRIPTION"] ?? "fiap-cloud-games-users";
            _processor = _sb.CreateProcessorWrapper(topic, subscription);
            _processor.ProcessMessageAsync += async args =>
            {
                var message = args.Message;

                using var activity = ServiceBusTracingHelper.StartConsumerActivity(
                    message,
                    "Users.PurchaseCompletedConsumer.Process",
                    topic,
                    subscription);

                var body = message.Body.ToString();
                var msg = JsonConvert.DeserializeObject<PurchaseCompletedEvent>(body);
                if (msg == null)
                {
                    _logger.LogWarning("PurchaseCompletedConsumer: mensagem inválida");
                    await args.CompleteMessageAsync(message);
                    return;
                }

                if (!msg.Success)
                {
                    _logger.LogInformation("PurchaseCompletedConsumer: compra não foi concluída com sucesso. PurchaseId: {PurchaseId}", msg.PurchaseId);
                    await args.CompleteMessageAsync(message);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IPurchaseMessageHandler>();
                await handler.HandleAsync(msg, args.CancellationToken);

                await args.CompleteMessageAsync(message);
            };

            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync(stoppingToken);
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            _logger.LogError($"PurchaseCompletedConsumer error: {arg.Exception}");
            return Task.CompletedTask;
        }


    }
}
