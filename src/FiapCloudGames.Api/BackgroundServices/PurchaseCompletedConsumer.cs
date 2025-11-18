using Azure.Messaging.ServiceBus;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Events;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using Newtonsoft.Json;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public class PurchaseCompletedConsumer : BackgroundService
    {
        private readonly ServiceBusClientWrapper _sb;
        private readonly IServiceProvider _provider;
        private ServiceBusProcessor? _processor;
        private readonly IConfiguration _config;

        public PurchaseCompletedConsumer(ServiceBusClientWrapper sb, IServiceProvider provider, IConfiguration config)
        {
            _sb = sb;
            _provider = provider;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queue = _config["PURCHASE_COMPLETED_QUEUE"] ?? "payments/purchases-completed";
            _processor = _sb.CreateProcessor(queue);
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync(stoppingToken);
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine($"PurchaseCompletedConsumer error: {arg.Exception}");
            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            var msg = JsonConvert.DeserializeObject<PurchaseCompletedEvent>(body);
            if (msg == null)
            {
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
