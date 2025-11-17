using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Application.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _repo;
        private readonly ILogger<GameService> _logger;

        public GameService(IGameRepository repo, ILogger<GameService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IEnumerable<GameDto>> GetAllAsync()
        {
            var games = await _repo.GetAllAsync();
            return games.Select(MapToDto);
        }

        public async Task<GameDto?> GetByIdAsync(int id)
        {
            var game = await _repo.GetByIdAsync(id);
            return game == null ? null : MapToDto(game);
        }

        public async Task<GameDto> CreateAsync(GameDto gameDto)
        {
            if (string.IsNullOrWhiteSpace(gameDto.Title))
                throw new ArgumentException("O título do jogo é obrigatório.");

            if (gameDto.Price <= 0)
                throw new ArgumentException("O preço do jogo deve ser maior que zero.");

            var game = MapToEntity(gameDto);
            var created = await _repo.CreateAsync(game);
            return MapToDto(created);
        }

        private static GameDto MapToDto(Game game) => new GameDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price
        };

        private static Game MapToEntity(GameDto dto) => new Game
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price
        };
    }
}