using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Application.Services;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<ILogger<GameService>> _mockLogger;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _mockGameRepository = new Mock<IGameRepository>();
            _mockLogger = new Mock<ILogger<GameService>>();
            _gameService = new GameService(_mockGameRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Cadastrar_WithValidGame_ShouldCallRepositoryAdd()
        {
            // Arrange
            var gameDto = new GameDto
            {
                Id = 1,
                Title = "Test Game",
                Description = "A test game description",
                Price = 59.99m,
            };

            var game = new Game
            {
                Id = 1,
                Title = "Test Game",
                Description = "A test game description",
                Price = 59.99m,
            };

            _mockGameRepository.Setup(repo => repo.CreateAsync(It.IsAny<Game>()))
                               .ReturnsAsync(game);

            // Act
            await _gameService.CreateAsync(gameDto);

            // Assert
            _mockGameRepository.Verify(repo => repo.CreateAsync(
                It.Is<Game>(g =>
                    g.Id == gameDto.Id &&
                    g.Title == gameDto.Title &&
                    g.Description == gameDto.Description &&
                    g.Price == gameDto.Price
                )), Times.Once);
        }

        [Fact]
        public async Task ObterPorId_WithValidId_ShouldReturnGame()
        {
            // Arrange
            var gameId = 1;
            var expectedGame = new Game
            {
                Id = gameId,
                Title = "Test Game",
                Description = "A test game description",
                Price = 59.99m,
            };

            _mockGameRepository.Setup(repo => repo.GetByIdAsync(gameId))
                              .ReturnsAsync(expectedGame);

            // Act
            var result = await _gameService.GetByIdAsync(gameId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new GameDto
            {
                Id = expectedGame.Id,
                Title = expectedGame.Title,
                Description = expectedGame.Description,
                Price = expectedGame.Price
            });
            result!.Id.Should().Be(gameId);
            result.Title.Should().Be("Test Game");
            _mockGameRepository.Verify(repo => repo.GetByIdAsync(gameId), Times.Once);
        }

        [Fact]
        public async Task ObterPorId_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var invalidId = 999;
            _mockGameRepository.Setup(repo => repo.GetByIdAsync(invalidId))
                              .ReturnsAsync((Game?)null);

            // Act
            var result = await _gameService.GetByIdAsync(invalidId);

            // Assert
            result.Should().BeNull();
            _mockGameRepository.Verify(repo => repo.GetByIdAsync(invalidId), Times.Once);
        }

        [Fact]
        public async Task ListarTodos_WithGamesInRepository_ShouldReturnAllGames()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game
                {
                    Id = 1,
                    Title = "Game 1",
                    Description = "First game",
                    Price = 29.99m,
                },
                new Game
                {
                    Id = 2,
                    Title = "Game 2",
                    Description = "Second game",
                    Price = 39.99m,
                },
                new Game
                {
                    Id = 3,
                    Title = "Game 3",
                    Description = "Third game",
                    Price = 49.99m,
                }
            };

            _mockGameRepository.Setup(repo => repo.GetAllAsync())
                              .ReturnsAsync(games);

            // Act
            var result = await _gameService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(games, options => options.ExcludingMissingMembers());
            _mockGameRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

    }
}
