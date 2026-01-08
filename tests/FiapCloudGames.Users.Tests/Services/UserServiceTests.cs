using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Publishers;
using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FiapCloudGames.Users.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly Mock<IUserEventPublisher> _publisher = new();
    private readonly Mock<ILogger<UserService>> _logger = new();

    private UserService CreateService() => new(_repo.Object, _logger.Object, _publisher.Object);

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Code = 1, Name = "U", Email = "u@x.com", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User };
        _repo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var svc = CreateService();

        // Act
        var result = await svc.GetByIdAsync(user.Id);

        // Assert
        result.ShouldBe(user);
        _repo.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
    }

    [Fact]
    public async Task GetByCodeAsync_DelegatesToRepository()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Code = 55, Name = "U2", Email = "u2@x.com", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User };
        _repo.Setup(r => r.GetByCodeAsync(user.Code)).ReturnsAsync(user);
        var svc = CreateService();

        // Act
        var result = await svc.GetByCodeAsync(user.Code);

        // Assert
        result.ShouldBe(user);
        _repo.Verify(r => r.GetByCodeAsync(user.Code), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_Throws_WhenEmailOrCodeExists()
    {
        // Arrange
        var existing = new User { Id = Guid.NewGuid(), Code = 2, Name = "X", Email = "x@x.com", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User };
        _repo.Setup(r => r.GetByEmailAndCodeAsync(existing.Email, existing.Code)).ReturnsAsync(existing);
        var svc = CreateService();

        var dto = new RegisterDto { Code = existing.Code, Name = "X", Email = existing.Email, Password = "P@ssword1", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User };

        // Act / Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await svc.CreateUserAsync(dto));
    }

    [Fact]
    public async Task CreateUserAsync_CreatesUser_WhenValid()
    {
        // Arrange
        _repo.Setup(r => r.GetByEmailAndCodeAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync((User?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var svc = CreateService();

        var dto = new RegisterDto { Code = 99, Name = "New", Email = "new@x.com", Password = "P@ssword1", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User };

        // Act
        var result = await svc.CreateUserAsync(dto);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe(dto.Email);
        result.Name.ShouldBe(dto.Name);
        result.Code.ShouldBe(dto.Code);
        // Ensure password hash was set
        result.PasswordHash.ShouldNotBeNullOrEmpty();
        _repo.Verify(r => r.CreateAsync(It.Is<User>(u => u.Email == dto.Email && u.Name == dto.Name && u.Code == dto.Code)), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        // Arrange
        var users = new[]
        {
            new User { Id = Guid.NewGuid(), Code = 1, Name = "U1", Email = "u1@x.com", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User },
            new User { Id = Guid.NewGuid(), Code = 2, Name = "U2", Email = "u2@x.com", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User }
        };

        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
        var svc = CreateService();

        // Act
        var result = await svc.GetAllAsync();

        // Assert
        result.ShouldBe(users);
        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
