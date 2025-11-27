using Azure.Messaging.ServiceBus;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Events;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using Newtonsoft.Json;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public class PurchaseCompletedConsumer : BackgroundService
    {
        private readonly IServiceBusClientWrapper _sb;
        private readonly IPurchaseMessageHandler _handler;
        private IServiceBusProcessor? _processor;
        private readonly IConfiguration _config;
        private readonly ILogger<PurchaseCompletedConsumer> _logger;

        public PurchaseCompletedConsumer(IServiceBusClientWrapper sb, IPurchaseMessageHandler handler, IConfiguration config, ILogger<PurchaseCompletedConsumer> logger)
        {
            _sb = sb;
            _handler = handler;
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
                var body = args.Message.Body.ToString();
                var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<PurchaseCompletedEvent>(body);
                if (msg == null)
                {
                    _logger.LogWarning("PurchaseCompletedConsumer: mensagem inválida"); 
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                if(!msg.Success)
                {
                    _logger.LogInformation("PurchaseCompletedConsumer: compra não foi concluída com sucesso. PurchaseId: {PurchaseId}", msg.PurchaseId);
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                await _handler.HandleAsync(msg, args.CancellationToken);

                await args.CompleteMessageAsync(args.Message);
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
