using System.Security.Claims;
using FiapCloudGames.Api.Controllers;
using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _userController;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _userController = new UserController(_mockUserService.Object);
        }

        #region GetUser Tests

        [Fact]
        public async Task GetUser_WithExistingUser_ShouldReturnOkWithUserData()
        {
            // Arrange
            var userId = 123;
            var expectedUser = new User
            {
                Id = 123,
                Name = "Test User",
                Email = "test@example.com",
                Role = UserRole.User
            };

            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userController.GetUser(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedData = okResult!.Value;
            returnedData.Should().NotBeNull();

            // Verificar se o objeto anônimo contém as propriedades esperadas
            var userProperties = returnedData!.GetType().GetProperties();
            userProperties.Should().HaveCount(4);

            var idProperty = returnedData.GetType().GetProperty("Id");
            idProperty!.GetValue(returnedData).Should().Be(expectedUser.Id);

            var nameProperty = returnedData.GetType().GetProperty("Name");
            nameProperty!.GetValue(returnedData).Should().Be(expectedUser.Name);

            var emailProperty = returnedData.GetType().GetProperty("Email");
            emailProperty!.GetValue(returnedData).Should().Be(expectedUser.Email);

            var roleProperty = returnedData.GetType().GetProperty("Role");
            roleProperty!.GetValue(returnedData).Should().Be(expectedUser.Role.ToString());

            _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUser_WithNonExistingUser_ShouldReturnNotFound()
        {
            // Arrange
            var userId = 999;
            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _userController.GetUser(userId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUser_WithAdministratorUser_ShouldReturnCorrectRole()
        {
            // Arrange
            var userId = 456;
            var adminUser = new User
            {
                Id = 456,
                Name = "Admin User",
                Email = "admin@example.com",
                Role = UserRole.Admin
            };

            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(adminUser);

            // Act
            var result = await _userController.GetUser(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedData = okResult!.Value;

            var roleProperty = returnedData!.GetType().GetProperty("Role");
            roleProperty!.GetValue(returnedData).Should().Be("Admin");
        }

        #endregion

        #region GetProfile Tests

        [Fact]
        public async Task GetProfile_WithAuthenticatedUser_ShouldReturnOkWithUserProfile()
        {
            // Arrange
            var userId = 123;
            var expectedUser = new User
            {
                Id = 123,
                Name = "Authenticated User",
                Email = "auth@example.com",
                Role = UserRole.User
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userController.GetProfile();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedData = okResult!.Value;
            returnedData.Should().NotBeNull();

            var idProperty = returnedData!.GetType().GetProperty("Id");
            idProperty!.GetValue(returnedData).Should().Be(expectedUser.Id);

            var nameProperty = returnedData.GetType().GetProperty("Name");
            nameProperty!.GetValue(returnedData).Should().Be(expectedUser.Name);

            var emailProperty = returnedData.GetType().GetProperty("Email");
            emailProperty!.GetValue(returnedData).Should().Be(expectedUser.Email);

            var roleProperty = returnedData.GetType().GetProperty("Role");
            roleProperty!.GetValue(returnedData).Should().Be(expectedUser.Role.ToString());

            _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetProfile_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            var principal = new ClaimsPrincipal(); // Sem claims

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _userController.GetProfile();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
            _mockUserService.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetProfile_WithAuthenticatedUserButUserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var userId = 999;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            _mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _userController.GetProfile();

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetProfile_WithEmptyUserIdClaim_ShouldReturnUnauthorized()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "") // Claim vazio
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _userController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _userController.GetProfile();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
            _mockUserService.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region CriarUser Tests

        [Fact]
        public async Task CriarUser_WithValidUser_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var user = new RegisterDto
            {
                Name = "New User",
                Email = "newuser@example.com",
                Role = UserRole.User,
                Password = "Ab234567##"
            };

            var createdUser = new User
            {
                Id = 1,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<RegisterDto>()))
                           .ReturnsAsync(createdUser);

            // Act
            var result = await _userController.CreateUser(user);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.ActionName.Should().Be(nameof(_userController.CreateUser));
            createdResult.RouteValues.Should().ContainKey("id");
            createdResult.RouteValues!["id"].Should().Be(createdUser.Id);

            var returnedData = createdResult.Value;
            returnedData.Should().NotBeNull();

            var idProperty = returnedData!.GetType().GetProperty("Id");
            idProperty!.GetValue(returnedData).Should().Be(createdUser.Id);

            var nameProperty = returnedData.GetType().GetProperty("Name");
            nameProperty!.GetValue(returnedData).Should().Be(user.Name);

            var emailProperty = returnedData.GetType().GetProperty("Email");
            emailProperty!.GetValue(returnedData).Should().Be(user.Email);

            var roleProperty = returnedData.GetType().GetProperty("Role");
            roleProperty!.GetValue(returnedData).Should().Be(user.Role.ToString());

            _mockUserService.Verify(s => s.CreateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task CriarUser_WithAdministratorRole_ShouldCreateSuccessfully()
        {
            // Arrange
            var user = new RegisterDto
            {
                Name = "New User",
                Email = "newuser@example.com",
                Role = UserRole.Admin,
                Password = "Ab234567##"
            };

            var createdUser = new User
            {
                Id = 2,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<RegisterDto>()))
                           .ReturnsAsync(createdUser);

            // Act
            var result = await _userController.CreateUser(user);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();

            var returnedData = createdResult!.Value;
            returnedData.Should().NotBeNull();

            var idProperty = returnedData!.GetType().GetProperty("Id");
            idProperty!.GetValue(returnedData).Should().Be(createdUser.Id);

            var roleProperty = returnedData.GetType().GetProperty("Role");
            roleProperty!.GetValue(returnedData).Should().Be("Admin");

            _mockUserService.Verify(s => s.CreateUserAsync(user), Times.Once);
        }
        #endregion

    }
}
