using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface ILibraryService
    {
        Task<IEnumerable<Library>> GetUserLibraryAsync(Guid userId);
        Task<bool> UserOwnsGameAsync(Guid userId, Guid gameId);
        Task<IEnumerable<Library>> GetLibrariesByPurchaseIdAsync(Guid id);
        Task<Library> CreateAsync(Library library);
    }
}
