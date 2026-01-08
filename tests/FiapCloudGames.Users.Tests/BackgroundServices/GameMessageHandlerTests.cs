using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using FiapCloudGames.Users.Api.BackgroundServices;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities.Events;

namespace FiapCloudGames.Tests.BackgroundServices
{
    public class GameMessageHandlerTests
    {
        [Fact]
        public async Task HandleAsync_Calls_GameService_ProcessAsync()
        {
            var mockGameService = new Mock<IGameService>();
            var mockLogger = new Mock<ILogger<GameMessageHandler>>();

            var handler = new GameMessageHandler(mockGameService.Object, mockLogger.Object);

            var message = new GameEvent(Guid.NewGuid(), 123, "Title", DateTimeOffset.UtcNow, null);
            var cts = new CancellationTokenSource();

            await handler.HandleAsync(message, cts.Token);

            mockGameService.Verify(s => s.ProcessAsync(It.Is<GameEvent>(g => g.Code == 123), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_When_GameService_Throws_Propagates_Exception_And_Logs()
        {
            var mockGameService = new Mock<IGameService>();
            var mockLogger = new Mock<ILogger<GameMessageHandler>>();

            var ex = new InvalidOperationException("boom");
            mockGameService.Setup(s => s.ProcessAsync(It.IsAny<GameEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(ex);

            var handler = new GameMessageHandler(mockGameService.Object, mockLogger.Object);
            var message = new GameEvent(Guid.NewGuid(), 7, "T", DateTimeOffset.UtcNow, null);

            await Should.ThrowAsync<InvalidOperationException>(async () => await handler.HandleAsync(message, CancellationToken.None));

            mockGameService.Verify(s => s.ProcessAsync(It.IsAny<GameEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
