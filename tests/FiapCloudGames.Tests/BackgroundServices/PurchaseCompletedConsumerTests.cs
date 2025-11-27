using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using FiapCloudGames.Users.Api.BackgroundServices;
using FiapCloudGames.Users.Infrastructure.ServiceBus;

namespace FiapCloudGames.Tests.BackgroundServices
{
    public class PurchaseCompletedConsumerTests
    {
        [Fact]
        public async Task ExecuteAsync_RegisterHandlers_And_StartsProcessor()
        {
            var mockClient = new Mock<IServiceBusClientWrapper>();
            var mockProcessor = new Mock<IServiceBusProcessor>();
            var mockHandler = new Mock<IPurchaseMessageHandler>();
            var mockLogger = new Mock<ILogger<PurchaseCompletedConsumer>>();

            mockClient.Setup(c => c.CreateProcessorWrapper(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ServiceBusProcessorOptions?>()))
                .Returns(mockProcessor.Object);

            mockProcessor.Setup(p => p.StartProcessingAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var config = new ConfigurationBuilder().Build();

            var consumer = new PurchaseCompletedConsumer(mockClient.Object, mockHandler.Object, config, mockLogger.Object);

            var cts = new CancellationTokenSource();
            await consumer.StartAsync(cts.Token);

            mockClient.Verify(c => c.CreateProcessorWrapper("payments-purchases-completed", "fiap-cloud-games-users", It.IsAny<ServiceBusProcessorOptions?>()), Times.Once);
            mockProcessor.VerifyAdd(p => p.ProcessMessageAsync += It.IsAny<Func<ProcessMessageEventArgs, Task>>(), Times.Once);
            mockProcessor.VerifyAdd(p => p.ProcessErrorAsync += It.IsAny<Func<ProcessErrorEventArgs, Task>>(), Times.Once);
            mockProcessor.Verify(p => p.StartProcessingAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
