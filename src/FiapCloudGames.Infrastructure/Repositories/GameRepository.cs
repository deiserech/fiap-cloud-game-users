using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Infrastructure.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GameRepository> _logger;

        public GameRepository(AppDbContext context, ILogger<GameRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Game?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Buscando jogo por ID: {Id}", id);
            return await _context.Games
                .Include(g => g.LibraryEntries)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Game> CreateAsync(Game game)
        {
            _logger.LogDebug("Criando jogo: {Title}", game.Title);
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(game.Id) ?? game;
        }

        public async Task<Game> UpdateAsync(Game game)
        {
            _logger.LogDebug("Atualizando jogo: {Title}", game.Title);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(game.Id) ?? game;
        }

        public async Task RemoveAsync(Guid id)
        {
            _logger.LogDebug("Deletando jogo por ID: {Id}", id);
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Games.AnyAsync(g => g.Id == id);
        }
    }
}
