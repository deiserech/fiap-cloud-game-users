using Azure.Messaging.ServiceBus;
using FiapCloudGames.Users.Domain.Entities.Events;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using FiapCloudGames.Users.Shared.Tracing;
using Newtonsoft.Json;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public class GameConsumer : BackgroundService
    {
        private readonly IServiceBusClientWrapper _sb;
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceBusProcessor? _processor;
        private readonly IConfiguration _config;
        private readonly ILogger<GameConsumer> _logger;

        public GameConsumer(IServiceBusClientWrapper sb, IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<GameConsumer> logger)
        {
            _sb = sb;
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _config["GAME_TOPIC"] ?? "games-created-updated";
            var subscription = _config["GAME_SUBSCRIPTION"] ?? "fiap-cloud-games-users";
            _processor = _sb.CreateProcessorWrapper(topic, subscription);
            _processor.ProcessMessageAsync += async args =>
            {
                var message = args.Message;

                using var activity = ServiceBusTracingHelper.StartConsumerActivity(
                    message,
                    "Users.GameConsumer.Process",
                    topic,
                    subscription);

                var body = message.Body.ToString();
                var msg = JsonConvert.DeserializeObject<GameEvent>(body);
                if (msg == null)
                {
                    await args.CompleteMessageAsync(message);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IGameMessageHandler>();
                await handler.HandleAsync(msg, args.CancellationToken);

                await args.CompleteMessageAsync(message);
            };

            _processor.ProcessErrorAsync += ErrorHandler;
            await _processor.StartProcessingAsync(stoppingToken);
        }

        private Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            _logger.LogError($"GameConsumer error: {arg.Exception}");
            return Task.CompletedTask;
        }
    }
}
