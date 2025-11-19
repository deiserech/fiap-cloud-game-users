using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Domain.Interfaces.Repositories
{
    public interface ILibraryRepository
    {
        Task<Library?> GetByIdAsync(Guid id);
        Task<Library?> GetByPurchaseGameAndUserAsync(Guid purchaseId, Guid gameId, Guid userId);
        Task<IEnumerable<Library>> GetByUserIdAsync(Guid userId);
        Task<Library> CreateAsync(Library library);
        Task<bool> ExistsAsync(Guid userId, Guid gameId);
    }
}
