using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Entities.Events;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FiapCloudGames.Users.Tests.Services;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _repo = new();
    private readonly Mock<ILogger<GameService>> _logger = new();

    private GameService CreateService() => new(_repo.Object, _logger.Object);

    [Fact]
    public async Task GetByCodeAsync_DelegatesToRepository()
    {
        // Arrange
        var expected = new Game(10, "T", DateTimeOffset.UtcNow, null);
        _repo.Setup(r => r.GetByCodeAsync(10)).ReturnsAsync(expected);
        var svc = CreateService();

        // Act
        var result = await svc.GetByCodeAsync(10);

        // Assert
        result.ShouldBe(expected);
        _repo.Verify(r => r.GetByCodeAsync(10), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_DoesNothing_WhenGameMissingAndRemovedAtSet()
    {
        // Arrange
        var msg = new GameEvent(Guid.NewGuid(), 11, "G", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        _repo.Setup(r => r.GetByCodeAsync(msg.Code)).ReturnsAsync((Game?)null);
        var svc = CreateService();

        // Act
        await svc.ProcessAsync(msg);

        // Assert
        _repo.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Never);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Game>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_DoesNothing_WhenMessageOlderThanSaved()
    {
        // Arrange
        var saved = new Game(12, "Old", DateTimeOffset.UtcNow.AddMinutes(10), null);
        var msg = new GameEvent(Guid.NewGuid(), 12, "New", DateTimeOffset.UtcNow, null);
        _repo.Setup(r => r.GetByCodeAsync(msg.Code)).ReturnsAsync(saved);
        var svc = CreateService();

        // Act
        await svc.ProcessAsync(msg);

        // Assert
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Game>()), Times.Never);
        _repo.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_CreatesGame_WhenMissingAndNotRemoved()
    {
        // Arrange
        var msg = new GameEvent(Guid.NewGuid(), 13, "CreateMe", DateTimeOffset.UtcNow, null);
        _repo.Setup(r => r.GetByCodeAsync(msg.Code)).ReturnsAsync((Game?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

        var svc = CreateService();

        // Act
        await svc.ProcessAsync(msg);

        // Assert
        _repo.Verify(r => r.CreateAsync(It.Is<Game>(g => g.Code == msg.Code && g.Title == msg.Title && g.UpdatedAt == msg.UpdatedAt && g.RemovedAt == msg.RemovedAt)), Times.Once);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Game>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_UpdatesGame_WhenExistsAndMessageIsNewer()
    {
        // Arrange
        var saved = new Game(14, "OldTitle", DateTimeOffset.UtcNow.AddMinutes(-10), null);
        saved.Id = Guid.NewGuid();
        var msg = new GameEvent(Guid.NewGuid(), 14, "UpdatedTitle", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        _repo.Setup(r => r.GetByCodeAsync(msg.Code)).ReturnsAsync(saved);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

        var svc = CreateService();

        // Act
        await svc.ProcessAsync(msg);

        // Assert
        _repo.Verify(r => r.UpdateAsync(It.Is<Game>(g => g.Code == msg.Code && g.Title == msg.Title && g.UpdatedAt == msg.UpdatedAt && g.RemovedAt == msg.RemovedAt && g.IsActive == (msg.RemovedAt == null))), Times.Once);
        _repo.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Never);
    }
}
