using System.IdentityModel.Tokens.Jwt;
using FiapCloudGames.Application.Services;
using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Enums;
using FiapCloudGames.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthService>> _mockLogger; 
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthService>>(); 

            SetupJwtConfiguration();

            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockConfiguration.Object,
                _mockLogger.Object 
            );
        }

        private void SetupJwtConfiguration()
        {
            var jwtSettingsSection = new Mock<IConfigurationSection>();
            jwtSettingsSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
            jwtSettingsSection.Setup(x => x["Audience"]).Returns("TestAudience");
            jwtSettingsSection.Setup(x => x["ExpiryInMinutes"]).Returns("60");

            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingsSection.Object);
            _mockConfiguration.Setup(x => x["JwtSettings:SecretKey"]).Returns("this-is-a-very-long-secret-key-that-is-at-least-32-characters-long");
        }

        #region Login Tests

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "validpassword"
            };

            var user = new User
            {
                Id = 1,
                Name = "Test User",
                Email = "test@example.com",
                Role = UserRole.User
            };
            user.SetPassword("validpassword");

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);

            // Act
            var result = await _authService.Login(loginDto);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(user.Email);
            result.Name.Should().Be(user.Name);
            result.UserId.Should().Be(user.Id);
            result.Token.Should().NotBeNullOrEmpty();

            // Verify JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.CanReadToken(result.Token).Should().BeTrue();

            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(loginDto.Email), Times.Once);
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_ShouldReturnNull()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "password"
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDto.Email)).ReturnsAsync((User?)null);

            // Act
            var result = await _authService.Login(loginDto);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(loginDto.Email), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var user = new User
            {
                Id = 1,
                Name = "Test User",
                Email = "test@example.com",
                Role = UserRole.User
            };
            user.SetPassword("correctpassword");

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);

            // Act
            var result = await _authService.Login(loginDto);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(loginDto.Email), Times.Once);
        }

        [Fact]
        public async Task Login_WithAdministratorUser_ShouldReturnValidAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "admin@example.com",
                Password = "adminpassword"
            };

            var adminUser = new User
            {
                Id = 2,
                Name = "Admin User",
                Email = "admin@example.com",
                Role = UserRole.Admin
            };
            adminUser.SetPassword("adminpassword");

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDto.Email)).ReturnsAsync(adminUser);

            // Act
            var result = await _authService.Login(loginDto);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(adminUser.Email);
            result.Name.Should().Be(adminUser.Name);
            result.UserId.Should().Be(adminUser.Id);
            result.Token.Should().NotBeNullOrEmpty();
        }
        #endregion

        #region Register Tests

        [Fact]
        public async Task Register_WithValidData_ShouldReturnAuthResponse()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "New User",
                Email = "newuser@example.com",
                Password = "newpassword123",
                Role = UserRole.User
            };

            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(registerDto.Email)).ReturnsAsync(false);

            // Mock CreateAsync para retornar o usuário com Id definido
            _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    u.Id = 10; // Simula atribuição de Id pelo repositório
                    return u;
                });

            // Act
            var result = await _authService.Register(registerDto);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(registerDto.Email);
            result.Name.Should().Be(registerDto.Name);
            result.UserId.Should().Be(10); // Agora espera o Id atribuído
            result.Token.Should().NotBeNullOrEmpty();

            // Verify JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.CanReadToken(result.Token).Should().BeTrue();

            _mockUserRepository.Verify(repo => repo.EmailExistsAsync(registerDto.Email), Times.Once);
            _mockUserRepository.Verify(repo => repo.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ShouldReturnNull()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "New User",
                Email = "existing@example.com",
                Password = "password123",
                Role = UserRole.User
            };

            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(registerDto.Email)).ReturnsAsync(true);

            // Act
            var result = await _authService.Register(registerDto);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(repo => repo.EmailExistsAsync(registerDto.Email), Times.Once);
            _mockUserRepository.Verify(repo => repo.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Register_WithAdministratorRole_ShouldCreateAdminUser()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Admin User",
                Email = "admin@example.com",
                Password = "adminpassword123",
                Role = UserRole.Admin
            };

            User? capturedUser = null;
            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(registerDto.Email)).ReturnsAsync(false);
            _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .Callback<User>(user => capturedUser = user)
                .ReturnsAsync((User u) =>
                {
                    u.Id = 20;
                    return u;
                });

            // Act
            var result = await _authService.Register(registerDto);

            // Assert
            result.Should().NotBeNull();
            capturedUser.Should().NotBeNull();
            capturedUser!.Role.Should().Be(UserRole.Admin);
            capturedUser.Name.Should().Be(registerDto.Name);
            capturedUser.Email.Should().Be(registerDto.Email);
            capturedUser.PasswordHash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_ShouldHashPassword()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "plaintextpassword",
                Role = UserRole.User
            };

            User? capturedUser = null;
            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(registerDto.Email)).ReturnsAsync(false);
            _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .Callback<User>(user => capturedUser = user)
                .ReturnsAsync((User u) =>
                {
                    u.Id = 30;
                    return u;
                });

            // Act
            var result = await _authService.Register(registerDto);

            // Assert
            result.Should().NotBeNull();
            capturedUser.Should().NotBeNull();
            capturedUser!.PasswordHash.Should().NotBe(registerDto.Password);
            capturedUser.PasswordHash.Should().NotBeNullOrEmpty();
            capturedUser.VerifyPassword(registerDto.Password).Should().BeTrue();
        }

        #endregion
    }
}
