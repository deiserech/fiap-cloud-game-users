using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Events;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Application.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ILibraryService _libraryService;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(ILibraryService libraryService, ILogger<PurchaseService> logger)
        {
            _libraryService = libraryService;
            _logger = logger;
        }

        public async Task ProcessAsync(PurchaseCompletedEvent message, CancellationToken cancellationToken = default)
        {
            var libraries = await _libraryService.GetLibraryByPurchaseGameAndUserAsync(message.PurchaseId, message.UserId, message.GameId);
            if (libraries is not null)
            {
                _logger.LogWarning("library still exists: {PurchaseId}, {GameId}, {UserId}", message.PurchaseId, message.GameId, message.UserId);
                return;
            }

            var library = new Library(
                message.UserId,
                message.GameId,
                message.PurchaseId,
                message.ProcessedAt
            );

            await _libraryService.CreateAsync(library);
            _logger.LogInformation("Library created for UserId: {UserId}, GameId: {GameId}, PurchaseId: {PurchaseId}", message.UserId, message.GameId, message.PurchaseId);
        }
    }
}
