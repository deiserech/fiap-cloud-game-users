using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities.Events;

namespace FiapCloudGames.Users.Api.BackgroundServices
{
    public class GameMessageHandler : IGameMessageHandler
    {
        private readonly IGameService _gameService;
        private readonly ILogger<GameMessageHandler> _logger;

        public GameMessageHandler(IGameService gameService, ILogger<GameMessageHandler> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        public async Task HandleAsync(GameEvent message, CancellationToken cancellationToken)
        {
            try
            {
                await _gameService.ProcessAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling game message");
                throw;
            }
        }
    }
}
