using FiapCloudGames.Users.Api.Controllers;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Enums;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService = new();

    private static AuthController CreateController(Mock<IAuthService> authService)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

        var controller = new AuthController(authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return controller;
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var controller = CreateController(_authService);
        controller.ModelState.AddModelError("Email", "Required");

        var dto = new LoginDto { Email = "a@a.com", Password = "P@ssword1" };

        // Act
        var result = await controller.Login(dto);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsInvalid()
    {
        // Arrange
        var dto = new LoginDto { Email = "no@user.com", Password = "wrong" };
        _authService.Setup(s => s.Login(dto)).ReturnsAsync((AuthResponseDto?)null);

        var controller = CreateController(_authService);

        // Act
        var result = await controller.Login(dto);

        // Assert
        var unauth = result as UnauthorizedObjectResult;
        unauth.ShouldNotBeNull();
        var unauthMsg = unauth!.Value?.ToString();
        unauthMsg.ShouldNotBeNull();
        unauthMsg.ShouldContain("Email ou senha inválidos");
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var dto = new LoginDto { Email = "user@x.com", Password = "P@ssword1" };
        var resp = new AuthResponseDto { Token = "t", Email = dto.Email, Name = "User" };
        _authService.Setup(s => s.Login(dto)).ReturnsAsync(resp);

        var controller = CreateController(_authService);

        // Act
        var result = await controller.Login(dto);

        // Assert
        var ok = result as OkObjectResult;
        ok.ShouldNotBeNull();
        var okVal = ok!.Value as AuthResponseDto;
        okVal.ShouldNotBeNull();
        okVal!.Email.ShouldBe(dto.Email);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var controller = CreateController(_authService);
        controller.ModelState.AddModelError("Name", "Required");

        var dto = new RegisterDto { Code = 1, Name = "x", Email = "x@x.com", Password = "P@ssword1", Role = UserRole.User };

        // Act
        var result = await controller.Register(dto);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange: choose password/email that fail ValidationHelper
        var dto = new RegisterDto { Code = 2, Name = "Bob", Email = "invalid-email", Password = "short", Role = UserRole.User };
        var controller = CreateController(_authService);

        // Act
        var result = await controller.Register(dto);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenEmailInUse()
    {
        // Arrange
        var dto = new RegisterDto { Code = 3, Name = "Jane", Email = "jane@x.com", Password = "P@ssword1", Role = UserRole.User };
        _authService.Setup(s => s.Register(dto)).ReturnsAsync((AuthResponseDto?)null);

        var controller = CreateController(_authService);

        // Act
        var result = await controller.Register(dto);

        // Assert
        var bad = result as BadRequestObjectResult;
        bad.ShouldNotBeNull();
        var badMsg = bad!.Value?.ToString();
        badMsg.ShouldNotBeNull();
        badMsg.ShouldContain("Email já está em uso");
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var dto = new RegisterDto { Code = 4, Name = "Sam", Email = "sam@x.com", Password = "P@ssword1", Role = UserRole.User };
        var resp = new AuthResponseDto { Token = "t", Email = dto.Email, Name = dto.Name };
        _authService.Setup(s => s.Register(dto)).ReturnsAsync(resp);

        var controller = CreateController(_authService);

        // Act
        var result = await controller.Register(dto);

        // Assert
        var ok = result as OkObjectResult;
        ok.ShouldNotBeNull();
        var okVal = ok!.Value as AuthResponseDto;
        okVal.ShouldNotBeNull();
        okVal!.Email.ShouldBe(dto.Email);
    }
}
