using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Events;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Application.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ILibraryService _libraryService;
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(ILibraryService libraryService, IGameService gameService, IUserService userService, ILogger<PurchaseService> logger)
        {
            _libraryService = libraryService;
            _gameService = gameService;
            _userService = userService;
            _logger = logger;
        }

        public async Task ProcessAsync(PurchaseCompletedEvent message, CancellationToken cancellationToken = default)
        {
            if (!message.Success)
            {
                _logger.LogInformation("Purchase not completed successfully: {PurchaseId}", message.PurchaseId);
                return;
            }

            var game = await _gameService.GetByCodeAsync(message.GameCode)
                ?? throw new Exception($"Game not found: {message.GameCode}");

            var user = await _userService.GetByCodeAsync(message.UserCode)
                ?? throw new Exception($"User not found: {message.UserCode}");

            var libraries = await _libraryService.GetLibraryByPurchaseGameAndUserAsync(message.PurchaseId!.Value, game.Id, user.Id);
            if (libraries is not null)
            {
                _logger.LogWarning("library still exists: {PurchaseId}, {GameCode}, {UserCode}", message.PurchaseId, message.GameCode, message.UserCode);
                return;
            }

            var library = new Library(
                user.Id,
                game.Id,
                message.PurchaseId!.Value,
                message.ProcessedAt!.Value
            );

            await _libraryService.CreateAsync(library);
            _logger.LogInformation("Library created for: {PurchaseId}, {GameCode}, {UserCode}", message.PurchaseId, message.GameCode, message.UserCode);
        }
    }
}
