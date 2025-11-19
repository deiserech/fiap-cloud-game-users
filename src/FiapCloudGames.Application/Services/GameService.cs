using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Entities.Events;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Application.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _repo;
        private readonly ILogger<GameService> _logger;

        public GameService(IGameRepository repo, ILogger<GameService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task ProcessAsync(GameEvent message, CancellationToken cancellationToken = default)
        {
            var game = await _repo.GetByIdAsync(message.Id);
            if (game is null && message.RemovedAt != null)
            {
                _logger.LogWarning("Game is removed: {GameCode}", message.Code);
                return;
            }

            if (game?.UpdatedAt > message.UpdatedAt)
            {
                _logger.LogWarning("Message is older then saved data: {GameCode}", message.Code);
                return;
            }

            if (game is null)
            {
                game = new Game(
                    message.Id,
                    message.Code,
                    message.Title,
                    message.UpdatedAt,
                    message.RemovedAt
                );

                await _repo.CreateAsync(game);
                _logger.LogInformation("Game created: {GameCode}", message.Code);
                return;
            }

            game.Code = message.Code;
            game.Title = message.Title;
            game.UpdatedAt = message.UpdatedAt;
            game.IsActive = message.RemovedAt == null;

            await _repo.UpdateAsync(game);
            _logger.LogInformation("Game updated: {GameCode}", message.Code);
            return;
        }
    }
}