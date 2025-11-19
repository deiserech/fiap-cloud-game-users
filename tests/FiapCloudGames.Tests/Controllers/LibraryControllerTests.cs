using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FiapCloudGames.Users.Api.Controllers;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Controllers
{
    public class LibraryControllerTests
    {
        private readonly Mock<ILibraryService> _mockLibraryService;
        private readonly LibraryController _libraryController;

        public LibraryControllerTests()
        {
            _mockLibraryService = new Mock<ILibraryService>();
            _libraryController = new LibraryController(_mockLibraryService.Object);
        }

        [Fact]
        public async Task GetUserLibrary_WithValidUserId_ShouldReturnOkWithLibrary()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedLibrary = new List<Library>
            {
                new Library(userId, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow)
            };

            _mockLibraryService.Setup(s => s.GetUserLibraryAsync(userId)).ReturnsAsync(expectedLibrary);

            // Act
            var result = await _libraryController.GetUserLibrary(userId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(expectedLibrary);
            _mockLibraryService.Verify(s => s.GetUserLibraryAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserLibrary_WhenUserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var errorMessage = "Usuário não encontrado";
            _mockLibraryService
                .Setup(s => s.GetUserLibraryAsync(userId))
                .ThrowsAsync(new ArgumentException(errorMessage));

            // Act
            var result = await _libraryController.GetUserLibrary(userId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.Value.Should().Be(errorMessage);
            _mockLibraryService.Verify(s => s.GetUserLibraryAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserLibrary_WhenUnexpectedErrorOccurs_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var errorMessage = "Erro inesperado";
            _mockLibraryService
                .Setup(s => s.GetUserLibraryAsync(userId))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _libraryController.GetUserLibrary(userId);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().Be(errorMessage);
            _mockLibraryService.Verify(s => s.GetUserLibraryAsync(userId), Times.Once);
        }
    }
}
