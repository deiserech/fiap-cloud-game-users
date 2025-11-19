using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Services
{
    public class PurchaseServiceTests
    {
        private readonly Mock<ILibraryService> _mockLibraryService;
        private readonly Mock<ILogger<PurchaseService>> _mockLogger;
        private readonly PurchaseService _purchaseService;

        public PurchaseServiceTests()
        {
            _mockLibraryService = new Mock<ILibraryService>();
            _mockLogger = new Mock<ILogger<PurchaseService>>();
            _purchaseService = new PurchaseService(_mockLibraryService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ProcessAsync_WhenLibraryExists_ShouldLogWarningAndNotCreateLibrary()
        {
            var message = new PurchaseCompletedEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                99.99m,
                "BRL",
                DateTimeOffset.UtcNow,
                Guid.NewGuid()
            );

            var existingLibrary = new Library(message.UserId, message.GameId, message.PurchaseId, message.ProcessedAt);

            _mockLibraryService
                .Setup(s => s.GetLibraryByPurchaseGameAndUserAsync(message.PurchaseId, message.GameId, message.UserId))
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
            var message = new PurchaseCompletedEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                99.99m,
                "BRL",
                DateTimeOffset.UtcNow,
                Guid.NewGuid()
            );

            _mockLibraryService
                .Setup(s => s.GetLibraryByPurchaseGameAndUserAsync(message.PurchaseId, message.GameId, message.UserId))
                .ReturnsAsync((Library?)null);

            _mockLibraryService
                .Setup(s => s.CreateAsync(It.IsAny<Library>()))
                .ReturnsAsync((Library l) => l);

            await _purchaseService.ProcessAsync(message);

            _mockLibraryService.Verify(
                s => s.CreateAsync(It.Is<Library>(l =>
                    l.UserId == message.UserId &&
                    l.GameId == message.GameId &&
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
