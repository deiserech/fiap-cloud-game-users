using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Entities.Events;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FiapCloudGames.Users.Tests.Services;

public class LibraryServiceTests
{
    private readonly Mock<ILibraryRepository> _libraryRepo = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<ILogger<LibraryService>> _logger = new();

    private LibraryService CreateService() => new(_libraryRepo.Object, _userService.Object, _logger.Object);

    [Fact]
    public async Task GetUserLibraryAsync_ReturnsLibrary_WhenUserExists()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Code = 7, Name = "U", Email = "u@x.com", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User };
        var entries = new[] { new Library(user.Id, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow) { Id = Guid.NewGuid(), User = user } };

        _userService.Setup(s => s.GetByCodeAsync(user.Code)).ReturnsAsync(user);
        _libraryRepo.Setup(r => r.GetByUserIdAsync(user.Id)).ReturnsAsync(entries);

        var svc = CreateService();

        // Act
        var result = await svc.GetUserLibraryAsync(user.Code);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(entries);
        _userService.Verify(s => s.GetByCodeAsync(user.Code), Times.Once);
        _libraryRepo.Verify(r => r.GetByUserIdAsync(user.Id), Times.Once);
    }

    [Fact]
    public async Task GetUserLibraryAsync_ThrowsArgumentException_WhenUserNotFound()
    {
        // Arrange
        _userService.Setup(s => s.GetByCodeAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
        var svc = CreateService();

        // Act / Assert
        await Should.ThrowAsync<ArgumentException>(async () => await svc.GetUserLibraryAsync(999));
    }

    [Fact]
    public async Task GetLibraryByPurchaseGameAndUserAsync_DelegatesToRepository()
    {
        // Arrange
        var lib = new Library(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow) { Id = Guid.NewGuid() };
        _libraryRepo.Setup(r => r.GetByPurchaseGameAndUserAsync(lib.PurchaseId, lib.GameId, lib.UserId)).ReturnsAsync(lib);
        var svc = CreateService();

        // Act
        var result = await svc.GetLibraryByPurchaseGameAndUserAsync(lib.PurchaseId, lib.GameId, lib.UserId);

        // Assert
        result.ShouldBe(lib);
        _libraryRepo.Verify(r => r.GetByPurchaseGameAndUserAsync(lib.PurchaseId, lib.GameId, lib.UserId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DelegatesToRepository()
    {
        // Arrange
        var lib = new Library(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow) { Id = Guid.NewGuid() };
        _libraryRepo.Setup(r => r.CreateAsync(lib)).ReturnsAsync(lib);
        var svc = CreateService();

        // Act
        var result = await svc.CreateAsync(lib);

        // Assert
        result.ShouldBe(lib);
        _libraryRepo.Verify(r => r.CreateAsync(lib), Times.Once);
    }
}
