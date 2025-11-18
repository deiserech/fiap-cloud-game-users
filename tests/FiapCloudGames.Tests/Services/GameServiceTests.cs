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
                Title = "Test Game",
                Description = "A test game description",
                Price = 59.99m,
            };

            var game = new Game
            {
                Id = Guid.NewGuid(),
                Title = "Test Game",
            };

            _mockGameRepository.Setup(repo => repo.CreateAsync(It.IsAny<Game>()))
                               .ReturnsAsync(game);

            // Act
            await _gameService.CreateAsync(gameDto);

            // Assert
            _mockGameRepository.Verify(repo => repo.CreateAsync(
                It.Is<Game>(g =>
                    g.Title == gameDto.Title
                )), Times.Once);
        }

        [Fact]
        public async Task ObterPorId_WithValidId_ShouldReturnGame()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var expectedGame = new Game
            {
                Title = "Test Game"
            };

            _mockGameRepository.Setup(repo => repo.GetByIdAsync(gameId))
                              .ReturnsAsync(expectedGame);

            // Act
            var result = await _gameService.GetByIdAsync(gameId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new GameDto
            {
                Title = expectedGame.Title
            });
            result.Title.Should().Be("Test Game");
            _mockGameRepository.Verify(repo => repo.GetByIdAsync(gameId), Times.Once);
        }

        [Fact]
        public async Task ObterPorId_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            _mockGameRepository.Setup(repo => repo.GetByIdAsync(invalidId))
                              .ReturnsAsync((Game?)null);

            // Act
            var result = await _gameService.GetByIdAsync(invalidId);

            // Assert
            result.Should().BeNull();
            _mockGameRepository.Verify(repo => repo.GetByIdAsync(invalidId), Times.Once);
        }
    }
}
