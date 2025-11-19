using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Domain.Enums;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Application.DTOs;

namespace FiapCloudGames.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<UserService>> _mockLogger; 
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<UserService>>(); 
            _userService = new UserService(_mockUserRepository.Object, _mockLogger.Object); 
        }

        #region ObterPorId Tests

        [Fact]
        public async Task TaskObterPorId_WithExistingUser_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                Role = UserRole.User,
                PasswordHash = "hashedpassword"
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedUser);
            result!.Id.Should().Be(expectedUser.Id);
            result.Name.Should().Be(expectedUser.Name);
            result.Email.Should().Be(expectedUser.Email);
            result.Role.Should().Be(expectedUser.Role);
            result.PasswordHash.Should().Be(expectedUser.PasswordHash);

            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task TaskObterPorId_WithNonExistingUser_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task TaskObterPorId_WithAdministratorUser_ShouldReturnCorrectRole()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin User",
                Email = "admin@example.com",
                Role = UserRole.Admin,
                PasswordHash = "adminhashedpassword"
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(adminUser);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.Role.Should().Be(UserRole.Admin);
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WithExistingUser_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository.Setup(repo => repo.ExistsAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _userService.ExistsAsync(userId);

            // Assert
            result.Should().BeTrue();
            _mockUserRepository.Verify(repo => repo.ExistsAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingUser_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository.Setup(repo => repo.ExistsAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _userService.ExistsAsync(userId);

            // Assert
            result.Should().BeFalse();
            _mockUserRepository.Verify(repo => repo.ExistsAsync(userId), Times.Once);
        }

        #endregion

        #region CreateUserAsync Tests

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUserAndReturnCreatedUser()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "New User",
                Email = "newuser@example.com",
                Password = "StrongPassword123!",
                Role = UserRole.User
            };

            var createdUser = new User
            {
                Id = Guid.NewGuid(),
                Name = registerDto.Name,
                Email = registerDto.Email,
                Role = registerDto.Role,
                PasswordHash = "hashedpassword"
            };

            _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<User>())).ReturnsAsync(createdUser);

            // Act
            var result = await _userService.CreateUserAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(registerDto.Name);
            result.Email.Should().Be(registerDto.Email);
            result.Role.Should().Be(registerDto.Role);
            _mockUserRepository.Verify(repo => repo.CreateAsync(It.Is<User>(u =>
                u.Name == registerDto.Name &&
                u.Email == registerDto.Email &&
                u.Role == registerDto.Role)), Times.Once);
        }

        #endregion


    }
}
