using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Domain.Interfaces.Repositories
{
    public interface ILibraryRepository
    {
        Task<Library?> GetByIdAsync(int id);
        Task<IEnumerable<Library>> GetByUserIdAsync(int userId);
        Task<Library> CreateAsync(Library library);
        Task<bool> ExistsAsync(int userId, int gameId);
    }
}
