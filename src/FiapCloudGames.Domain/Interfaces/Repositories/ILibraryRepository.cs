using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Domain.Interfaces.Repositories
{
    public interface ILibraryRepository
    {
        Task<Library?> GetByIdAsync(Guid id);
        Task<IEnumerable<Library>> GetByPurchaseIdAsync(Guid purchaseId);
        Task<IEnumerable<Library>> GetByUserIdAsync(Guid userId);
        Task<Library> CreateAsync(Library library);
        Task<bool> ExistsAsync(Guid userId, Guid gameId);
    }
}
