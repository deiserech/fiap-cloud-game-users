using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Entities.Events;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;

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
        public async Task ProcessAsync_WhenGameIsRemovedAndNotExists_ShouldLogWarningAndReturn()
        {
            // Arrange
            var message = new GameEvent(
                Guid.NewGuid(),
                123,
                "Test Game",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow
            );

            _mockGameRepository.Setup(r => r.GetByIdAsync(message.Id)).ReturnsAsync((Game?)null);

            // Act
            await _gameService.ProcessAsync(message);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Game is removed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockGameRepository.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Never);
            _mockGameRepository.Verify(r => r.UpdateAsync(It.IsAny<Game>()), Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenMessageIsOlderThanSavedData_ShouldLogWarningAndReturn()
        {
            // Arrange
            var message = new GameEvent(
                Guid.NewGuid(),
                123,
                "Test Game",
                DateTimeOffset.UtcNow.AddDays(-1),
                null
            );

            var existingGame = new Game(message.Id, 123, "Test Game", DateTimeOffset.UtcNow, null);

            _mockGameRepository.Setup(r => r.GetByIdAsync(message.Id)).ReturnsAsync(existingGame);

            // Act
            await _gameService.ProcessAsync(message);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Message is older then saved data")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockGameRepository.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Never);
            _mockGameRepository.Verify(r => r.UpdateAsync(It.IsAny<Game>()), Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenGameDoesNotExist_ShouldCreateGameAndLogInformation()
        {
            // Arrange
            var message = new GameEvent(
                Guid.NewGuid(),
                123,
                "Test Game",
                DateTimeOffset.UtcNow,
                null
            );

            _mockGameRepository.Setup(r => r.GetByIdAsync(message.Id)).ReturnsAsync((Game?)null);

            // Act
            await _gameService.ProcessAsync(message);

            // Assert
            _mockGameRepository.Verify(r => r.CreateAsync(It.Is<Game>(g =>
                g.Id == message.Id &&
                g.Code == message.Code &&
                g.Title == message.Title &&
                g.UpdatedAt == message.UpdatedAt &&
                g.IsActive == true)), Times.Once);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Game created")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_WhenGameExistsAndMessageIsNewer_ShouldUpdateGameAndLogInformation()
        {
            // Arrange
            var message = new GameEvent(
                Guid.NewGuid(),
                123,
                "Updated Title",
                DateTimeOffset.UtcNow,
                null
            );

            var existingGame = new Game(message.Id, 123, "Old Title", DateTimeOffset.UtcNow.AddDays(-1), null);

            _mockGameRepository.Setup(r => r.GetByIdAsync(message.Id)).ReturnsAsync(existingGame);

            // Act
            await _gameService.ProcessAsync(message);

            // Assert
            _mockGameRepository.Verify(r => r.UpdateAsync(It.Is<Game>(g =>
                g.Id == message.Id &&
                g.Code == message.Code &&
                g.Title == message.Title &&
                g.UpdatedAt == message.UpdatedAt &&
                g.IsActive == true)), Times.Once);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Game updated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
