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
        private readonly IServiceProvider _provider;
        private ServiceBusProcessor? _processor;
        private readonly IConfiguration _config;
        private readonly ILogger<PurchaseCompletedConsumer> _logger;

        public PurchaseCompletedConsumer(IServiceBusClientWrapper sb, IServiceProvider provider, IConfiguration config, ILogger<PurchaseCompletedConsumer> logger)
        {
            _sb = sb;
            _provider = provider;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _config["PURCHASE_TOPIC"] ?? "payments-purchases-completed";
            var subscription = _config["PURCHASE_SUBSCRIPTION"] ?? "fiap-cloud-games-users";
            _processor = _sb.CreateProcessor(topic, subscription);
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync(stoppingToken);
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            _logger.LogError($"PurchaseCompletedConsumer error: {arg.Exception}");
            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            var msg = JsonConvert.DeserializeObject<PurchaseCompletedEvent>(body);
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


            using var scope = _provider.CreateScope();
            var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
            await purchaseService.ProcessAsync(msg, args.CancellationToken);

            await args.CompleteMessageAsync(args.Message);
        }
    }
}
