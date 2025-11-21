using Microsoft.Extensions.Logging;
using FiapCloudGames.Users.Shared.Tracing;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Application.DTOs;

namespace FiapCloudGames.Users.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repo, ILogger<UserService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(UserService)}.GetByIdAsync");
            _logger.LogInformation("Buscando usuário por ID: {Id}", id);
            return await _repo.GetByIdAsync(id);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(UserService)}.ExistsAsync");
            _logger.LogInformation("Verificando existência do usuário: {Id}", id);
            return await _repo.ExistsAsync(id);
        }

        public async Task<User> CreateUserAsync(RegisterDto registerDto)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(UserService)}.CreateUserAsync");
            _logger.LogInformation("Criando usuário: {Email}", registerDto.Email);
            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Role = registerDto.Role
            };

            user.SetPassword(registerDto.Password);

            var created = await _repo.CreateAsync(user);
            _logger.LogInformation("Usuário criado com sucesso: {Email}", registerDto.Email);
            return created;
        }
        public async Task<User?> GetByCodeAsync(int code)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(UserService)}.GetByCodeAsync");
            _logger.LogInformation("Buscando usuário por código: {Code}", code);
            // Implemente GetByCodeAsync no IUserRepository
            return await _repo.GetByCodeAsync(code);
        }

    }
}