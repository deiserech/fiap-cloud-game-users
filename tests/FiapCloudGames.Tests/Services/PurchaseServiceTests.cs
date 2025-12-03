using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Events;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FiapCloudGames.Tests.Services;

public class PurchaseServiceTests
{
    private readonly Mock<ILibraryService> _libraryService = new();
    private readonly Mock<IGameService> _gameService = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<ILogger<PurchaseService>> _logger = new();

    private PurchaseService CreateService() => new(_libraryService.Object, _gameService.Object, _userService.Object, _logger.Object);

    [Fact]
    public async Task ProcessAsync_Throws_WhenGameNotFound()
    {
        // Arrange
        var evt = new PurchaseCompletedEvent(Guid.NewGuid(), 1, 2, DateTimeOffset.UtcNow, true);
        _gameService.Setup(g => g.GetByCodeAsync(evt.GameCode)).ReturnsAsync((Game?)null);
        var svc = CreateService();

        // Act / Assert
        var ex = await Should.ThrowAsync<Exception>(async () => await svc.ProcessAsync(evt));
        ex.Message.ShouldContain(evt.GameCode.ToString());
    }

    [Fact]
    public async Task ProcessAsync_Throws_WhenUserNotFound()
    {
        // Arrange
        var evt = new PurchaseCompletedEvent(Guid.NewGuid(), 10, 20, DateTimeOffset.UtcNow, true);
        var game = new Game(20, "G", DateTimeOffset.UtcNow, null) { Id = Guid.NewGuid() };
        _gameService.Setup(g => g.GetByCodeAsync(evt.GameCode)).ReturnsAsync(game);
        _userService.Setup(u => u.GetByCodeAsync(evt.UserCode)).ReturnsAsync((User?)null);

        var svc = CreateService();

        // Act / Assert
        var ex = await Should.ThrowAsync<Exception>(async () => await svc.ProcessAsync(evt));
        ex.Message.ShouldContain(evt.UserCode.ToString());
    }

    [Fact]
    public async Task ProcessAsync_DoesNotCreate_WhenLibraryExists()
    {
        // Arrange
        var evt = new PurchaseCompletedEvent(Guid.NewGuid(), 100, 200, DateTimeOffset.UtcNow, true);
        var game = new Game(200, "G", DateTimeOffset.UtcNow, null) { Id = Guid.NewGuid() };
        var user = new User { Id = Guid.NewGuid(), Code = 100, Name = "U", Email = "u@x.com", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User };

        _gameService.Setup(g => g.GetByCodeAsync(evt.GameCode)).ReturnsAsync(game);
        _userService.Setup(u => u.GetByCodeAsync(evt.UserCode)).ReturnsAsync(user);
        _libraryService.Setup(l => l.GetLibraryByPurchaseGameAndUserAsync(evt.PurchaseId!.Value, game.Id, user.Id)).ReturnsAsync(new Library(user.Id, game.Id, evt.PurchaseId!.Value, evt.ProcessedAt!.Value));

        var svc = CreateService();

        // Act
        await svc.ProcessAsync(evt);

        // Assert
        _libraryService.Verify(l => l.CreateAsync(It.IsAny<Library>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_CreatesLibrary_WhenNotExists()
    {
        // Arrange
        var evt = new PurchaseCompletedEvent(Guid.NewGuid(), 333, 444, DateTimeOffset.UtcNow, true);
        var game = new Game(444, "Game", DateTimeOffset.UtcNow, null) { Id = Guid.NewGuid() };
        var user = new User { Id = Guid.NewGuid(), Code = 333, Name = "U2", Email = "u2@x.com", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User };

        _gameService.Setup(g => g.GetByCodeAsync(evt.GameCode)).ReturnsAsync(game);
        _userService.Setup(u => u.GetByCodeAsync(evt.UserCode)).ReturnsAsync(user);
        _libraryService.Setup(l => l.GetLibraryByPurchaseGameAndUserAsync(evt.PurchaseId!.Value, game.Id, user.Id)).ReturnsAsync((Library?)null);
        _libraryService.Setup(l => l.CreateAsync(It.IsAny<Library>())).ReturnsAsync((Library lib) => lib);

        var svc = CreateService();

        // Act
        await svc.ProcessAsync(evt);

        // Assert
        _libraryService.Verify(l => l.CreateAsync(It.Is<Library>(lib => lib.PurchaseId == evt.PurchaseId && lib.GameId == game.Id && lib.UserId == user.Id && lib.AcquiredAt == evt.ProcessedAt)), Times.Once);
    }
}
