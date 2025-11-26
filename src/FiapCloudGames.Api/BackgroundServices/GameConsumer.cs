using Azure.Messaging.ServiceBus;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities.Events;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using Newtonsoft.Json;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public class GameConsumer : BackgroundService
    {
        private readonly IServiceBusClientWrapper _sb;
        private readonly IServiceProvider _provider;
        private ServiceBusProcessor? _processor;
        private readonly IConfiguration _config;

        public GameConsumer(IServiceBusClientWrapper sb, IServiceProvider provider, IConfiguration config)
        {
            _sb = sb;
            _provider = provider;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queue = _config["GAME_QUEUE"] ?? "games/created-updated";
            _processor = _sb.CreateProcessor(queue);
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync(stoppingToken);
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine($"GameConsumer error: {arg.Exception}");
            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            var msg = JsonConvert.DeserializeObject<GameEvent>(body);
            if (msg == null)
            {
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            using var scope = _provider.CreateScope();
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
            await gameService.ProcessAsync(msg, args.CancellationToken);

            await args.CompleteMessageAsync(args.Message);
        }
    }
}
