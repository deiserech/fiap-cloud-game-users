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

        public async Task<IEnumerable<Library>> GetUserLibraryAsync(int code)
        {
            var user = await _userService.GetByCodeAsync(code)
                ?? throw new ArgumentException($"Usuário com código {code} não encontrado.");

            return await _libraryRepository.GetByUserIdAsync(user.Id);
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
