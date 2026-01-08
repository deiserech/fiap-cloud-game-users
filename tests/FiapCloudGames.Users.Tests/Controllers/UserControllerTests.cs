using System.Security.Claims;
using FiapCloudGames.Users.Api.Controllers;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Enums;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _service = new();

    private static UserController CreateController(Mock<IUserService> service, ClaimsPrincipal? user = null, IConfiguration? configuration = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

        configuration ??= new ConfigurationBuilder().Build();

        var controller = new UserController(service.Object, configuration)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        if (user != null)
            controller.ControllerContext.HttpContext.User = user;

        return controller;
    }

    [Fact]
    public async Task GetUsers_ReturnsOk_WithUserList()
    {
        // Arrange
        var users = new List<User>
            {
                new() { Id = Guid.NewGuid(), Code = 1, Name = "User1", Email = "u1@example.com", Role = UserRole.Admin },
                new() { Id = Guid.NewGuid(), Code = 2, Name = "User2", Email = "u2@example.com", Role = UserRole.User }
            };

        _service.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

        // Simule a configuração da chave interna
        var configDict = new Dictionary<string, string>
            {
                { "InternalApiKeys:GetUsers", "test-key" }
            };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "Admin")], authenticationType: "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var controller = CreateController(_service, principal, configuration);

        // Adicione o header X-Internal-Api-Key
        controller.ControllerContext.HttpContext.Request.Headers["X-Internal-Api-Key"] = "test-key";

        // Act
        var result = await controller.GetUsers();

        // Assert
        var ok = result as OkObjectResult;
        ok.ShouldNotBeNull();
        var okVal = ok!.Value as IEnumerable<UserDto>;
        okVal.ShouldNotBeNull();
        okVal!.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetUser_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Code = 42, Name = "Jon", Email = "jon@example.com", Role = UserRole.User };
        _service.Setup(s => s.GetByCodeAsync(42)).ReturnsAsync(user);

        var controller = CreateController(_service);

        // Act
        var result = await controller.GetUser(42);

        // Assert
        var ok = result as OkObjectResult;
        ok.ShouldNotBeNull();
        var okVal = ok!.Value as UserDto;
        okVal.ShouldNotBeNull();
        okVal!.Code.ShouldBe(42);
    }

    [Fact]
    public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _service.Setup(s => s.GetByCodeAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
        var controller = CreateController(_service);

        // Act
        var result = await controller.GetUser(99);

        // Assert
        var notFound = result as NotFoundObjectResult;
        notFound.ShouldNotBeNull();
        var pd = notFound!.Value as Microsoft.AspNetCore.Mvc.ProblemDetails;
        pd.ShouldNotBeNull();
        pd!.Title.ShouldNotBeNull();
        pd.Title.ShouldContain("Usuário não cadastrado");
    }

    [Fact]
    public async Task GetProfile_ReturnsUnauthorized_WhenNoNameIdentifierClaim()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "a@a.com") });
        var principal = new ClaimsPrincipal(identity);
        var controller = CreateController(_service, principal);

        // Act
        var result = await controller.GetProfile();

        // Assert
        var unauthorized = result as UnauthorizedObjectResult;
        unauthorized.ShouldNotBeNull();
        var pd = unauthorized!.Value as Microsoft.AspNetCore.Mvc.ProblemDetails;
        pd.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetProfile_ReturnsNotFound_WhenUserMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) });
        var principal = new ClaimsPrincipal(identity);

        _service.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((User?)null);
        var controller = CreateController(_service, principal);

        // Act
        var result = await controller.GetProfile();

        // Assert
        var notFound = result as NotFoundObjectResult;
        notFound.ShouldNotBeNull();
        var pd = notFound!.Value as Microsoft.AspNetCore.Mvc.ProblemDetails;
        pd.ShouldNotBeNull();
        pd!.Title.ShouldNotBeNull();
        pd.Title.ShouldContain("Erro ao buscar perfil do usuário");
    }

    [Fact]
    public async Task GetProfile_ReturnsOk_WhenUserFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User { Id = id, Code = 7, Name = "Ana", Email = "ana@example.com", Role = UserRole.Admin };
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) });
        var principal = new ClaimsPrincipal(identity);

        _service.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(user);
        var controller = CreateController(_service, principal);

        // Act
        var result = await controller.GetProfile();

        // Assert
        var ok = result as OkObjectResult;
        ok.ShouldNotBeNull();
        var okVal = ok!.Value as UserDto;
        okVal.ShouldNotBeNull();
        okVal!.Code.ShouldBe(7);
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var controller = CreateController(_service);
        controller.ModelState.AddModelError("Name", "Required");

        // Act
        var invalidRegister = new RegisterDto
        {
            Code = 1,
            Name = "x",
            Email = "x@x.com",
            Password = "pass1234",
            Role = UserRole.User
        };

        var result = await controller.CreateUser(invalidRegister);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateUser_ReturnsCreated_WhenSuccess()
    {
        // Arrange
        var register = new RegisterDto
        {
            Code = 100,
            Name = "New User",
            Email = "new@user.com",
            Password = "password123",
            Role = UserRole.User
        };

        var created = new User { Id = Guid.NewGuid(), Code = 100, Name = register.Name, Email = register.Email, Role = register.Role };
        _service.Setup(s => s.CreateUserAsync(register)).ReturnsAsync(created);

        var controller = CreateController(_service);

        // Act
        var result = await controller.CreateUser(register);

        // Assert
        var createdResult = result as CreatedAtActionResult;
        createdResult.ShouldNotBeNull();
        createdResult!.RouteValues.ShouldNotBeNull();
        createdResult.RouteValues.ContainsKey("code").ShouldBeTrue();
        createdResult.RouteValues["code"].ShouldBe(created.Code);
        var createdVal = createdResult.Value as UserDto;
        createdVal.ShouldNotBeNull();
        createdVal!.Code.ShouldBe(100);
    }
}
