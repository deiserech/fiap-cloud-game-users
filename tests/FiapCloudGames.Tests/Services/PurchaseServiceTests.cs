using System;
using System.Threading.Tasks;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Enums;
using FiapCloudGames.Users.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Services
{
    public class PurchaseServiceTests
    {
        private readonly Mock<ILibraryService> _mockLibraryService;
        private readonly Mock<IGameService> _mockGameService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<PurchaseService>> _mockLogger;
        private readonly PurchaseService _purchaseService;

        public PurchaseServiceTests()
        {
            _mockLibraryService = new Mock<ILibraryService>();
            _mockGameService = new Mock<IGameService>();
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<PurchaseService>>();
            _purchaseService = new PurchaseService(
                _mockLibraryService.Object,
                _mockGameService.Object,
                _mockUserService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task ProcessAsync_WhenLibraryExists_ShouldLogWarningAndNotCreateLibrary()
        {
            var gameCode = 123;
            var userCode = 456;
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var message = new PurchaseCompletedEvent(
                Guid.NewGuid(), 
                userCode,       
                gameCode,       
                DateTimeOffset.UtcNow, 
                true                    
            );

            var game = new Game(gameId, gameCode, "Test Game", DateTimeOffset.UtcNow, null);
            var user = new User { Id = userId, Code = userCode, Name = "User", Email = "user@email.com", Role = UserRole.User };

            var existingLibrary = new Library(userId, gameId, message.PurchaseId, message.ProcessedAt);

            _mockGameService.Setup(s => s.GetByCodeAsync(gameCode)).ReturnsAsync(game);
            _mockUserService.Setup(s => s.GetByCodeAsync(userCode)).ReturnsAsync(user);
            _mockLibraryService
                .Setup(s => s.GetLibraryByPurchaseGameAndUserAsync(message.PurchaseId, gameId, userId))
                .ReturnsAsync(existingLibrary);

            await _purchaseService.ProcessAsync(message);

            _mockLibraryService.Verify(s => s.CreateAsync(It.IsAny<Library>()), Times.Never);
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("library still exists")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_WhenLibraryDoesNotExist_ShouldCreateLibraryAndLogInformation()
        {
            var gameCode = 123;
            var userCode = 456;
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var message = new PurchaseCompletedEvent(
                Guid.NewGuid(), 
                userCode,       
                gameCode,       
                DateTimeOffset.UtcNow, 
                true  
            );

            var game = new Game(gameId, gameCode, "Test Game", DateTimeOffset.UtcNow, null);
            var user = new User { Id = userId, Code = userCode, Name = "User", Email = "user@email.com", Role = UserRole.User };

            _mockGameService.Setup(s => s.GetByCodeAsync(gameCode)).ReturnsAsync(game);
            _mockUserService.Setup(s => s.GetByCodeAsync(userCode)).ReturnsAsync(user);
            _mockLibraryService
                .Setup(s => s.GetLibraryByPurchaseGameAndUserAsync(message.PurchaseId, gameId, userId))
                .ReturnsAsync((Library?)null);

            _mockLibraryService
                .Setup(s => s.CreateAsync(It.IsAny<Library>()))
                .ReturnsAsync((Library l) => l);

            await _purchaseService.ProcessAsync(message);

            _mockLibraryService.Verify(
                s => s.CreateAsync(It.Is<Library>(l =>
                    l.UserId == userId &&
                    l.GameId == gameId &&
                    l.PurchaseId == message.PurchaseId &&
                    l.AcquiredAt == message.ProcessedAt)),
                Times.Once);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Library created for UserId")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
