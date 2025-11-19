using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Services
{
    public class LibraryServiceTests
    {
        private readonly Mock<ILibraryRepository> _mockLibraryRepository;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<LibraryService>> _mockLogger;
        private readonly LibraryService _libraryService;

        public LibraryServiceTests()
        {
            _mockLibraryRepository = new Mock<ILibraryRepository>();
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<LibraryService>>();
            _libraryService = new LibraryService(_mockLibraryRepository.Object, _mockUserService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUserLibraryAsync_WithExistingUser_ShouldReturnLibraryGames()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedGames = new List<Library>
            {
                new Library(userId, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow)
            };

            _mockLibraryRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(expectedGames);

            // Act
            var result = await _libraryService.GetUserLibraryAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(expectedGames);
            _mockLibraryRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetLibraryByPurchaseGameAndUserAsync_WithExistingLibrary_ShouldReturnLibrary()
        {
            // Arrange
            var purchaseId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedLibrary = new Library(userId, gameId, purchaseId, DateTimeOffset.UtcNow);

            _mockLibraryRepository.Setup(r => r.GetByPurchaseGameAndUserAsync(purchaseId, gameId, userId)).ReturnsAsync(expectedLibrary);

            // Act
            var result = await _libraryService.GetLibraryByPurchaseGameAndUserAsync(purchaseId, gameId, userId);

            // Assert
            result.Should().BeEquivalentTo(expectedLibrary);
            _mockLibraryRepository.Verify(r => r.GetByPurchaseGameAndUserAsync(purchaseId, gameId, userId), Times.Once);
        }

        [Fact]
        public async Task UserOwnsGameAsync_WhenExists_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            _mockLibraryRepository.Setup(r => r.ExistsAsync(userId, gameId)).ReturnsAsync(true);

            // Act
            var result = await _libraryService.UserOwnsGameAsync(userId, gameId);

            // Assert
            result.Should().BeTrue();
            _mockLibraryRepository.Verify(r => r.ExistsAsync(userId, gameId), Times.Once);
        }

        [Fact]
        public async Task UserOwnsGameAsync_WhenNotExists_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            _mockLibraryRepository.Setup(r => r.ExistsAsync(userId, gameId)).ReturnsAsync(false);

            // Act
            var result = await _libraryService.UserOwnsGameAsync(userId, gameId);

            // Assert
            result.Should().BeFalse();
            _mockLibraryRepository.Verify(r => r.ExistsAsync(userId, gameId), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateLibraryAndReturnIt()
        {
            // Arrange
            var library = new Library(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

            _mockLibraryRepository.Setup(r => r.CreateAsync(library)).ReturnsAsync(library);

            // Act
            var result = await _libraryService.CreateAsync(library);

            // Assert
            result.Should().BeEquivalentTo(library);
            _mockLibraryRepository.Verify(r => r.CreateAsync(library), Times.Once);
        }
    }
}
