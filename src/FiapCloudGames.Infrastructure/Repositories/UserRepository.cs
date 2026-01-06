using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Infrastructure.Repositories
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

        public async Task<User?> GetByIdAsync(Guid id)
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

        public async Task<User?> GetByEmailAndCodeAsync(string email, int code)
        {
            _logger.LogDebug("Buscando usuário por email e código: {Email}", email);
            return await _context.Users
                .Include(u => u.LibraryGames)
                    .ThenInclude(l => l.Game)
                .FirstOrDefaultAsync(u => u.Email == email || u.Code == code);
        }

        public async Task<User> CreateAsync(User user)
        {
            _logger.LogDebug("Criando usuário: {Email}", user.Email);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(user.Id) ?? user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByCodeAsync(int code)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Code == code);
        }

        public async Task<IReadOnlyCollection<User>> GetAllAsync()
        {
            _logger.LogDebug("Buscando lista de usuários");
            var users = await _context.Users.AsNoTracking().ToListAsync();
            return users;
        }

    }
}
