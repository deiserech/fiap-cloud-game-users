using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Shared.Tracing;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Application.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IUserService _userService;
        private readonly ILogger<LibraryService> _logger;

        public LibraryService(
            ILibraryRepository libraryRepository,
            IUserService userService,
            ILogger<LibraryService> logger)
        {
            _libraryRepository = libraryRepository;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IEnumerable<Library>> GetUserLibraryAsync(Guid userId)
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

        public async Task<Library?> GetLibraryByPurchaseGameAndUserAsync(Guid purchaseId, Guid gameId, Guid userId)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(LibraryService)}.GetLibraryEntryAsync");
            return await _libraryRepository.GetByPurchaseGameAndUserAsync(purchaseId, gameId, userId);
        }

        public async Task<bool> UserOwnsGameAsync(Guid userId, Guid gameId)
        {
            return await _libraryRepository.ExistsAsync(userId, gameId);
        }

        public async Task<Library> CreateAsync(Library library)
        {
            return await _libraryRepository.CreateAsync(library);
        }

    }
}
