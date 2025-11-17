using FiapCloudGames.Shared.Tracing;
using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Interfaces.Repositories;
using FiapCloudGames.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Application.Services
{
    public class UserService : Interfaces.Services.IUserService
    {
        private readonly Domain.Interfaces.Repositories.IUserRepository _repo;
        private readonly ILogger<UserService> _logger;

        public UserService(Domain.Interfaces.Repositories.IUserRepository repo, ILogger<UserService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(UserService)}.GetByIdAsync");
            _logger.LogInformation("Buscando usuário por ID: {Id}", id);
            return await _repo.GetByIdAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
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
    }
}