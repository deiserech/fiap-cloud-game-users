using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Interfaces.Repositories;
using FiapCloudGames.Shared.Tracing;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Application.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IUserService _userService;
        private readonly IGameService _gameService;
        private readonly IPromotionService _promotionService;
        private readonly ILogger<LibraryService> _logger;

        public LibraryService(
            ILibraryRepository libraryRepository,
            IUserService userService,
            IGameService gameService,
            IPromotionService promotionService,
            ILogger<LibraryService> logger)
        {
            _libraryRepository = libraryRepository;
            _userService = userService;
            _gameService = gameService;
            _promotionService = promotionService;
            _logger = logger;
        }


        public async Task<IEnumerable<Library>> GetUserLibraryAsync(int userId)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(LibraryService)}.GetUserLibraryAsync");
            _logger.LogInformation("Buscando biblioteca do usuário: {UserId}", userId);
            if (!await _userService.ExistsAsync(userId))
            {
                _logger.LogWarning("Usuário não encontrado: {UserId}", userId);
                throw new ArgumentException("Usuário não encontrado.");
            }

            return await _libraryRepository.GetByUserIdAsync(userId);
        }


        public async Task<Library?> GetLibraryEntryAsync(int id)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(LibraryService)}.GetLibraryEntryAsync");
            _logger.LogInformation("Buscando entrada da biblioteca: {Id}", id);
            return await _libraryRepository.GetByIdAsync(id);
        }

        public async Task<Library> PurchaseGameAsync(int userId, int gameId)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(LibraryService)}.PurchaseGameAsync");
            _logger.LogInformation("Usuário {UserId} está tentando comprar o jogo {GameId}", userId, gameId);
            if (!await _userService.ExistsAsync(userId))
            {
                _logger.LogWarning("Usuário não encontrado: {UserId}", userId);
                throw new ArgumentException("Usuário não encontrado.");
            }

            var game = await _gameService.GetByIdAsync(gameId);
            if (game == null)
            {
                _logger.LogWarning("Jogo não encontrado: {GameId}", gameId);
                throw new ArgumentException("Jogo não encontrado.");
            }

            if (await UserOwnsGameAsync(userId, gameId))
            {
                _logger.LogWarning("Usuário {UserId} já possui o jogo {GameId}", userId, gameId);
                throw new InvalidOperationException("Usuário já possui este jogo em sua biblioteca.");
            }

            var finalPrice = await _promotionService.GetDiscountedPriceAsync(gameId);

            if (finalPrice <= 0)
            {
                _logger.LogWarning("Preço de compra inválido para o jogo {GameId}: {Price}", gameId, finalPrice);
                throw new ArgumentException("Preço de compra deve ser maior que zero.");
            }

            var library = new Library
            {
                UserId = userId,
                GameId = gameId,
            };

            return await _libraryRepository.CreateAsync(library);
        }

        public async Task<bool> UserOwnsGameAsync(int userId, int gameId)
        {
            return await _libraryRepository.ExistsAsync(userId, gameId);
        }
    }
}
