using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface ILibraryService
    {
        Task<Library?> GetLibraryByPurchaseGameAndUserAsync(Guid purchaseId, Guid gameId, Guid userId);
        Task<bool> UserOwnsGameAsync(Guid userId, Guid gameId);
        Task<Library> CreateAsync(Library library);
        Task<IEnumerable<Library>> GetUserLibraryAsync(Guid userId);
    }
}
