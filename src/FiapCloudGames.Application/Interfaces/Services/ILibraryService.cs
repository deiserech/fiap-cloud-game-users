using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Application.Interfaces.Services
{
    public interface ILibraryService
    {
        Task<IEnumerable<Library>> GetUserLibraryAsync(int userId);
        Task<Library?> GetLibraryEntryAsync(int id);
        Task<Library> PurchaseGameAsync(int userId, int gameId);
        Task<bool> UserOwnsGameAsync(int userId, int gameId);
    }
}
