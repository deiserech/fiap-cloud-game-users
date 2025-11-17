using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Interfaces.Repositories;
using FiapCloudGames.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Buscando usuário por ID: {Id}", id);
            return await _context.Users
                .Include(u => u.LibraryGames)
                    .ThenInclude(l => l.Game)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogDebug("Buscando usuário por email: {Email}", email);
            return await _context.Users
                .Include(u => u.LibraryGames)
                    .ThenInclude(l => l.Game)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            _logger.LogDebug("Listando todos os usuários");
            return await _context.Users
                .Include(u => u.LibraryGames)
                    .ThenInclude(l => l.Game)
                .ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _logger.LogDebug("Criando usuário: {Email}", user.Email);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(user.Id) ?? user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _logger.LogDebug("Atualizando usuário: {Email}", user.Email);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(user.Id) ?? user;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogDebug("Deletando usuário por ID: {Id}", id);
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}
