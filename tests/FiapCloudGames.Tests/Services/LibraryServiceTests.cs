using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Application.Services;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace FiapCloudGames.Tests.Services
{
    public class LibraryServiceTests
    {
        private readonly Mock<ILibraryRepository> _mockLibraryRepo;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IGameService> _mockGameService;
        private readonly Mock<IPromotionService> _mockPromotionService;
        private readonly Mock<ILogger<LibraryService>> _mockLogger;
        private readonly LibraryService _libraryService;

        public LibraryServiceTests()
        {
            _mockLibraryRepo = new Mock<ILibraryRepository>();
            _mockUserService = new Mock<IUserService>();
            _mockGameService = new Mock<IGameService>();
            _mockPromotionService = new Mock<IPromotionService>();
            _mockLogger = new Mock<ILogger<LibraryService>>();
            _libraryService = new LibraryService(
                _mockLibraryRepo.Object,
                _mockUserService.Object,
                _mockGameService.Object,
                _mockPromotionService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetUserLibraryAsync_WithValidUserId_ReturnsLibraryEntries()
        {
            // Arrange
            int userId = 1;
            var expectedLibraries = new List<Library>
                {
                    new Library { Id = 1, UserId = userId, GameId = 10 },
                    new Library { Id = 2, UserId = userId, GameId = 20 }
                };

            _mockUserService.Setup(r => r.ExistsAsync(userId)).ReturnsAsync(true);
            _mockLibraryRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(expectedLibraries);

            // Act
            var result = await _libraryService.GetUserLibraryAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(expectedLibraries);
            _mockUserService.Verify(r => r.ExistsAsync(userId), Times.Once);
            _mockLibraryRepo.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetLibraryEntryAsync_WithValidId_ReturnsLibraryEntry()
        {
            // Arrange
            int entryId = 1;
            var expectedEntry = new Library { Id = entryId, UserId = 1, GameId = 10 };

            _mockLibraryRepo.Setup(r => r.GetByIdAsync(entryId)).ReturnsAsync(expectedEntry);

            // Act
            var result = await _libraryService.GetLibraryEntryAsync(entryId);

            // Assert
            result.Should().Be(expectedEntry);
            _mockLibraryRepo.Verify(r => r.GetByIdAsync(entryId), Times.Once);
        }

        [Fact]
        public async Task PurchaseGameAsync_WithValidData_CreatesLibraryEntry()
        {
            // Arrange
            int userId = 1;
            int gameId = 10;
            decimal gamePrice = 99.99m;
            decimal discountedPrice = 79.99m;
            var game = new GameDto { Id = gameId, Price = gamePrice };
            var createdLibrary = new Library { Id = 1, UserId = userId, GameId = gameId };

            _mockUserService.Setup(r => r.ExistsAsync(userId)).ReturnsAsync(true);
            _mockGameService.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockLibraryRepo.Setup(r => r.ExistsAsync(userId, gameId)).ReturnsAsync(false);
            _mockPromotionService.Setup(s => s.GetDiscountedPriceAsync(gameId)).ReturnsAsync(discountedPrice);
            _mockLibraryRepo.Setup(r => r.CreateAsync(It.IsAny<Library>())).ReturnsAsync(createdLibrary);

            // Act
            var result = await _libraryService.PurchaseGameAsync(userId, gameId);

            // Assert
            result.Should().Be(createdLibrary);
            _mockUserService.Verify(r => r.ExistsAsync(userId), Times.Once);
            _mockGameService.Verify(r => r.GetByIdAsync(gameId), Times.Once);
            _mockLibraryRepo.Verify(r => r.ExistsAsync(userId, gameId), Times.Once);
            _mockPromotionService.Verify(s => s.GetDiscountedPriceAsync(gameId), Times.Once);
            _mockLibraryRepo.Verify(r => r.CreateAsync(It.IsAny<Library>()), Times.Once);
        }
    }
}
