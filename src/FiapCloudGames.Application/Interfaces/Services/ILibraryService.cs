using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Application.Interfaces.Services
{
    public interface ILibraryService
    {
        Task<IEnumerable<Library>> GetUserLibraryAsync(Guid userId);
        Task<bool> UserOwnsGameAsync(Guid userId, Guid gameId);
        Task<IEnumerable<Library>> GetLibrariesByPurchaseIdAsync(Guid id);
        Task<Library> CreateAsync(Library library);
    }
}
