using FiapCloudGames.Api.Controllers;
using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _authController = new AuthController(_mockAuthService.Object);
        }


        #region Login Tests

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOkWithAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "validpassword"
            };

            var expectedResponse = new AuthResponseDto
            {
                Token = "valid-jwt-token",
                Email = "test@example.com",
                Name = "Test User",
                UserId = 1
            };

            _mockAuthService.Setup(s => s.Login(loginDto)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
            _mockAuthService.Verify(s => s.Login(loginDto), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            _mockAuthService.Setup(s => s.Login(loginDto)).ReturnsAsync((AuthResponseDto?)null);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult!.Value.Should().BeEquivalentTo(new { message = "Email ou senha inválidos" });
            _mockAuthService.Verify(s => s.Login(loginDto), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "invalid-email",
                Password = "password"
            };

            _authController.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().BeOfType<SerializableError>();
            _mockAuthService.Verify(s => s.Login(It.IsAny<LoginDto>()), Times.Never);
        }

        #endregion

        #region Register Tests

        [Fact]
        public async Task Register_WithValidData_ShouldReturnOkWithAuthResponse()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "New User",
                Email = "newuser@example.com",
                Password = "ValidPassword123!",
                Role = UserRole.User
            };

            var expectedResponse = new AuthResponseDto
            {
                Token = "valid-jwt-token",
                Email = "newuser@example.com",
                Name = "New User",
                UserId = 1
            };

            _mockAuthService.Setup(s => s.Register(registerDto)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
            _mockAuthService.Verify(s => s.Register(registerDto), Times.Once);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "New User",
                Email = "existing@example.com",
                Password = "ValidPassword123!",
                Role = UserRole.User
            };

            _mockAuthService.Setup(s => s.Register(registerDto)).ReturnsAsync((AuthResponseDto?)null);

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().BeEquivalentTo(new { message = "Email já está em uso" });
            _mockAuthService.Verify(s => s.Register(registerDto), Times.Once);
        }

        [Fact]
        public async Task Register_WithInvalidEmailFormat_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Test User",
                Email = "invalid-email-format",
                Password = "ValidPassword123!",
                Role = UserRole.User
            };

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().BeOfType<SerializableError>();
            var errors = badRequestResult.Value as SerializableError;

            errors.Should().ContainKey("RegisterDto");
            var registerDtoErrors = errors!["RegisterDto"] as string[];
            registerDtoErrors.Should().Contain("O email fornecido não é válido.");

            _mockAuthService.Verify(s => s.Register(It.IsAny<RegisterDto>()), Times.Never);
        }

        [Theory]
        [InlineData("weak")]
        [InlineData("password")]
        [InlineData("PASSWORD")]
        [InlineData("12345678")]
        [InlineData("password123")]
        [InlineData("PASSWORD123")]
        [InlineData("Password")]
        public async Task Register_WithWeakPassword_ShouldReturnBadRequest(string weakPassword)
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = weakPassword,
                Role = UserRole.User
            };

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().BeOfType<SerializableError>();

            var errors = badRequestResult.Value as SerializableError;
            errors.Should().ContainKey("RegisterDto");
            _mockAuthService.Verify(s => s.Register(It.IsAny<RegisterDto>()), Times.Never);
        }

        [Theory]
        [InlineData("ValidPass123!")]
        [InlineData("MyStr0ng@Password")]
        [InlineData("Complex#Pass123")]
        public async Task Register_WithStrongPassword_ShouldCallAuthService(string strongPassword)
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = strongPassword,
                Role = UserRole.User
            };

            _mockAuthService.Setup(s => s.Register(registerDto)).ReturnsAsync((AuthResponseDto?)null);

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            _mockAuthService.Verify(s => s.Register(registerDto), Times.Once);
        }

        [Fact]
        public async Task Register_WithAdministratorRole_ShouldReturnOkWithAuthResponse()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "Admin User",
                Email = "admin@example.com",
                Password = "AdminPassword123!",
                Role = UserRole.Admin
            };

            var expectedResponse = new AuthResponseDto
            {
                Token = "admin-jwt-token",
                Email = "admin@example.com",
                Name = "Admin User",
                UserId = 2
            };

            _mockAuthService.Setup(s => s.Register(registerDto)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
            _mockAuthService.Verify(s => s.Register(registerDto), Times.Once);
        }

        #endregion

    }
}