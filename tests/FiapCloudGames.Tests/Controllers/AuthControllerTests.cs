using FiapCloudGames.Users.Api.Controllers;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Enums;
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
                Name = "Test User"
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
                Name = "New User"
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
        public async Task Register_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Name = "",
                Email = "invalid-email",
                Password = "short",
                Role = UserRole.User
            };

            _authController.ModelState.AddModelError("Name", "O nome é obrigatório.");
            _authController.ModelState.AddModelError("Email", "Formato de e-mail inválido.");
            _authController.ModelState.AddModelError("Password", "A senha deve ter entre 8 e 128 caracteres.");

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().BeOfType<SerializableError>();
            _mockAuthService.Verify(s => s.Register(It.IsAny<RegisterDto>()), Times.Never);
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
                Name = "Admin User"
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
