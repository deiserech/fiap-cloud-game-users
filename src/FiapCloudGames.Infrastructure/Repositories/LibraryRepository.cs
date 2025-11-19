using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Infrastructure.Repositories
{
    public class LibraryRepository : ILibraryRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LibraryRepository> _logger;

        public LibraryRepository(AppDbContext context, ILogger<LibraryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Library?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Buscando entrada da biblioteca por ID: {Id}", id);
            return await _context.Libraries
                .Include(l => l.User)
                .Include(l => l.Game)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Library>> GetByPurchaseIdAsync(Guid purchaseId)
        {
            _logger.LogDebug("Buscando biblioteca da compra: {purchaseId}", purchaseId);
            return await _context.Libraries
                .Where(l => l.PurchaseId == purchaseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Library>> GetByUserIdAsync(Guid userId)
        {
            _logger.LogDebug("Buscando biblioteca do usuário: {UserId}", userId);
            return await _context.Libraries
                .Include(l => l.User)
                .Include(l => l.Game)
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task<Library> CreateAsync(Library library)
        {
            _logger.LogDebug("Criando entrada na biblioteca para usuário {UserId} e jogo {GameId}", library.UserId, library.GameId);
            _context.Libraries.Add(library);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(library.Id) ?? library;
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid gameId)
        {
            return await _context.Libraries
                .AnyAsync(l => l.UserId == userId && l.GameId == gameId);
        }
    }
}
