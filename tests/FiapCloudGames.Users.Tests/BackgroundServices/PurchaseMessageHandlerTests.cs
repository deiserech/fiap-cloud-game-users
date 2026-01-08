using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using FiapCloudGames.Users.Api.BackgroundServices;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Events;

namespace FiapCloudGames.Tests.BackgroundServices
{
    public class PurchaseMessageHandlerTests
    {
        [Fact]
        public async Task HandleAsync_Calls_PurchaseService_ProcessAsync()
        {
            var mockPurchaseService = new Mock<IPurchaseService>();
            var mockLogger = new Mock<ILogger<PurchaseMessageHandler>>();

            var handler = new PurchaseMessageHandler(mockPurchaseService.Object, mockLogger.Object);

            var message = new PurchaseCompletedEvent(Guid.NewGuid(), 1, 2, DateTimeOffset.UtcNow, true);

            await handler.HandleAsync(message, CancellationToken.None);

            mockPurchaseService.Verify(s => s.ProcessAsync(It.Is<PurchaseCompletedEvent>(m => m.PurchaseId == message.PurchaseId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_When_PurchaseService_Throws_Propagates_Exception_And_Logs()
        {
            var mockPurchaseService = new Mock<IPurchaseService>();
            var mockLogger = new Mock<ILogger<PurchaseMessageHandler>>();

            var ex = new InvalidOperationException("boom");
            mockPurchaseService.Setup(s => s.ProcessAsync(It.IsAny<PurchaseCompletedEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(ex);

            var handler = new PurchaseMessageHandler(mockPurchaseService.Object, mockLogger.Object);
            var message = new PurchaseCompletedEvent(Guid.NewGuid(), 5, 6, DateTimeOffset.UtcNow, true);

            await Should.ThrowAsync<InvalidOperationException>(async () => await handler.HandleAsync(message, CancellationToken.None));

            mockPurchaseService.Verify(s => s.ProcessAsync(It.IsAny<PurchaseCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
