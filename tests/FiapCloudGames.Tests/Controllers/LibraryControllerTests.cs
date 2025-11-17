using FiapCloudGames.Api.Controllers;
using FiapCloudGames.Api.Request;
using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Controllers
{
    public class LibraryControllerTests
    {
        private readonly Mock<ILibraryService> _mockLibraryService;
        private readonly LibraryController _controller;

        public LibraryControllerTests()
        {
            _mockLibraryService = new Mock<ILibraryService>();
            _controller = new LibraryController(_mockLibraryService.Object);
        }

        [Fact]
        public async Task GetUserLibrary_WithValidUserId_ReturnsOkWithLibraryEntries()
        {
            // Arrange
            int userId = 1;
            var expectedLibraries = new List<Library>
            {
                new Library { Id = 1, UserId = userId, GameId = 10 },
                new Library { Id = 2, UserId = userId, GameId = 20 }
            };

            _mockLibraryService.Setup(s => s.GetUserLibraryAsync(userId)).ReturnsAsync(expectedLibraries);

            // Act
            var result = await _controller.GetUserLibrary(userId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedLibraries);
            _mockLibraryService.Verify(s => s.GetUserLibraryAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetLibraryEntry_WithValidId_ReturnsOkWithLibraryEntry()
        {
            // Arrange
            int entryId = 1;
            var expectedEntry = new Library { Id = entryId, UserId = 1, GameId = 10 };

            _mockLibraryService.Setup(s => s.GetLibraryEntryAsync(entryId)).ReturnsAsync(expectedEntry);

            // Act
            var result = await _controller.GetLibraryEntry(entryId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().Be(expectedEntry);
            _mockLibraryService.Verify(s => s.GetLibraryEntryAsync(entryId), Times.Once);
        }

        [Fact]
        public async Task PurchaseGame_WithValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new PurchaseGameRequest
            {
                UserId = 1,
                GameId = 10
            };

            var createdLibrary = new Library { Id = 1, UserId = request.UserId, GameId = request.GameId };

            _mockLibraryService.Setup(s => s.PurchaseGameAsync(
                request.UserId,
                request.GameId)).ReturnsAsync(createdLibrary);

            // Act
            var result = await _controller.PurchaseGame(request);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult!.Value.Should().Be(createdLibrary);
            _mockLibraryService.Verify(s => s.PurchaseGameAsync(
                request.UserId,
                request.GameId), Times.Once);
        }
    }
}
