using FiapCloudGames.Users.Api.Controllers;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FiapCloudGames.Tests.Controllers;

public class SuggestionsControllerTests
{
    private readonly Mock<ISuggestionService> _suggestionService = new();

    private static SuggestionsController CreateController(Mock<ISuggestionService> suggestionService)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

        var controller = new SuggestionsController(suggestionService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetSuggestions_ReturnsNotFound_WhenServiceReturnsNull()
    {
        // Arrange
        _suggestionService
            .Setup(s => s.GetSuggestionsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.FromResult<IEnumerable<GameSuggestionDto>>(null!));

        var controller = CreateController(_suggestionService);

        // Act
        var result = await controller.GetSuggestions(10);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSuggestions_ReturnsNotFound_WhenServiceReturnsEmpty()
    {
        // Arrange
        _suggestionService
            .Setup(s => s.GetSuggestionsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Empty<GameSuggestionDto>());

        var controller = CreateController(_suggestionService);

        // Act
        var result = await controller.GetSuggestions(11);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSuggestions_ReturnsOk_WhenServiceReturnsItems()
    {
        // Arrange
        var suggestions = new List<GameSuggestionDto>
        {
            new GameSuggestionDto
            {
                GameId = System.Guid.NewGuid(),
                GameCode = 42,
                Title = "Test Game",
                Category = FiapCloudGames.Users.Domain.Enums.GameCategory.Action
            }
        };

        _suggestionService
            .Setup(s => s.GetSuggestionsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(suggestions);

        var controller = CreateController(_suggestionService);

        // Act
        var result = await controller.GetSuggestions(12);

        // Assert
        var ok = result as OkObjectResult;
        ok.ShouldNotBeNull();
        var body = ok!.Value as IEnumerable<GameSuggestionDto>;
        body.ShouldNotBeNull();
        body!.Count().ShouldBe(1);
        body.First().GameCode.ShouldBe(42);
    }
}
