using FiapCloudGames.Users.Api.Controllers;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;
using Xunit;

namespace FiapCloudGames.Tests.Controllers;

public class LibraryControllerTests
{
    private readonly Mock<ILibraryService> _libraryService = new();

    private static LibraryController CreateController(Mock<ILibraryService> libraryService)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

        var controller = new LibraryController(libraryService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetUserLibrary_ReturnsNotFound_WhenServiceReturnsNull()
    {
        // Arrange
        _libraryService.Setup(s => s.GetUserLibraryAsync(It.IsAny<int>())).Returns(Task.FromResult<IEnumerable<Library>>(null!));
        var controller = CreateController(_libraryService);

        // Act
        var result = await controller.GetUserLibrary(10);

        // Assert
        var notFound = result as NotFoundObjectResult;
        notFound.ShouldNotBeNull();
        var pd = notFound!.Value as Microsoft.AspNetCore.Mvc.ProblemDetails;
        pd.ShouldNotBeNull();
        pd!.Title.ShouldNotBeNull();
        pd.Title.ShouldContain("Biblioteca vazia");
    }

    [Fact]
    public async Task GetUserLibrary_ReturnsNotFound_WhenServiceReturnsEmpty()
    {
        // Arrange
        _libraryService.Setup(s => s.GetUserLibraryAsync(It.IsAny<int>())).ReturnsAsync(Enumerable.Empty<Library>());
        var controller = CreateController(_libraryService);

        // Act
        var result = await controller.GetUserLibrary(11);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserLibrary_ReturnsOk_WhenLibraryHasItems()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Code = 5, Name = "Player", Email = "p@p.com", Role = FiapCloudGames.Users.Domain.Enums.UserRole.User };
        var game = new Game { Id = Guid.NewGuid(), Code = 55, Title = "Space Invaders", UpdatedAt = System.DateTimeOffset.UtcNow, RemovedAt = null };
        var lib = new Library(user.Id, game.Id, Guid.NewGuid(), System.DateTimeOffset.UtcNow)
        {
            Id = Guid.NewGuid(),
            User = user,
            Game = game
        };

        _libraryService.Setup(s => s.GetUserLibraryAsync(user.Code)).ReturnsAsync(new[] { lib });
        var controller = CreateController(_libraryService);

        // Act
        var result = await controller.GetUserLibrary(user.Code);

        // Assert
        var ok = result as OkObjectResult;
        ok.ShouldNotBeNull();
        var dtos = ok!.Value as IEnumerable<LibraryDto>;
        dtos.ShouldNotBeNull();
        dtos!.First().GameCode.ShouldBe(game.Code);
        dtos.First().UserCode.ShouldBe(user.Code);
    }
}
