using System.Collections.Generic;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Publishers;
using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Enums;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FiapCloudGames.Users.Tests.Services;

public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepo = new();
    private Mock<ILogger<AuthService>> _logger = new();
    private Mock<IUserEventPublisher> _userEventPublisher = new();


    private IConfiguration BuildConfiguration(string secretKey)
    {
        var inMemory = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = secretKey,
            ["JwtSettings:Issuer"] = "unit-tests",
            ["JwtSettings:Audience"] = "unit-tests-aud",
            ["JwtSettings:ExpiryInMinutes"] = "60",
        };

        return new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
    }

    [Fact]
    public async Task Login_ReturnsNull_WhenUserNotFound()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var svc = new AuthService(_userRepo.Object, BuildConfiguration(new string('a', 32)), _logger.Object, _userEventPublisher.Object);

        var result = await svc.Login(new LoginDto { Email = "no@u.com", Password = "x" });

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Login_ReturnsNull_WhenPasswordInvalid()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@x.com", Role = UserRole.User };
        // PasswordHash is empty -> VerifyPassword returns false
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var svc = new AuthService(_userRepo.Object, BuildConfiguration(new string('a', 32)), _logger.Object, _userEventPublisher.Object);

        var result = await svc.Login(new LoginDto { Email = user.Email, Password = "wrong" });

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Login_ReturnsAuthResponse_WhenSuccess()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@x.com", Role = UserRole.Admin };
        user.SetPassword("P@ssword1");

        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var svc = new AuthService(_userRepo.Object, BuildConfiguration(new string('b', 64)), _logger.Object, _userEventPublisher.Object);

        var result = await svc.Login(new LoginDto { Email = user.Email, Password = "P@ssword1" });

        result.ShouldNotBeNull();
        result!.Email.ShouldBe(user.Email);
        result.Token.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_Throws_WhenSecretTooShort()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@x.com", Role = UserRole.Admin };
        user.SetPassword("P@ssword1");
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var svc = new AuthService(_userRepo.Object, BuildConfiguration("short"), _logger.Object, _userEventPublisher.Object);

        await Should.ThrowAsync<InvalidOperationException>(async () => await svc.Login(new LoginDto { Email = user.Email, Password = "P@ssword1" }));
    }

    [Fact]
    public async Task Register_ReturnsNull_WhenEmailExists()
    {
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var svc = new AuthService(_userRepo.Object, BuildConfiguration(new string('c', 32)), _logger.Object, _userEventPublisher.Object);

        var result = await svc.Register(new RegisterDto { Code = 1, Name = "X", Email = "e@x.com", Password = "P@ssword1", Role = UserRole.User });

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Register_ReturnsAuthResponse_WhenSuccess()
    {
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var svc = new AuthService(_userRepo.Object, BuildConfiguration(new string('d', 64)), _logger.Object, _userEventPublisher.Object);

        var dto = new RegisterDto { Code = 2, Name = "New", Email = "new@x.com", Password = "P@ssword1", Role = UserRole.User };

        var result = await svc.Register(dto);

        result.ShouldNotBeNull();
        result!.Email.ShouldBe(dto.Email);
        result.Token.ShouldNotBeNullOrEmpty();
        _userRepo.Verify(r => r.CreateAsync(It.Is<User>(u => u.Email == dto.Email && u.Name == dto.Name)), Times.Once);
    }
}
