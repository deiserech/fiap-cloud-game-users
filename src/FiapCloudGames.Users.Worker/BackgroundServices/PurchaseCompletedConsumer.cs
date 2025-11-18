using Azure.Messaging.ServiceBus;
using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using FiapCloudGames.Users.Worker.Events;
using Newtonsoft.Json;

namespace FiapCloudGames.Users.Application.BackgroundServices
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
            var libraryService = scope.ServiceProvider.GetRequiredService<ILibraryService>();

            var libraries = await libraryService.GetLibrariesByPurchaseIdAsync(msg.PurchaseId);
            if (!libraries.Any())
            {
                //TODO: log
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            var library = new Library
            (
                msg.UserId,
                msg.GameId,
                msg.PurchaseId,
                msg.ProcessedAt
            );

            await libraryService.CreateAsync(library);
            //todo: log

            await args.CompleteMessageAsync(args.Message);
        }
    }
}
