using System;
using System.Threading;
using System.Threading.Tasks;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Domain.Enums;
using FiapCloudGames.Users.Infrastructure.Elasticsearch;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using Shouldly;
using Xunit;

namespace FiapCloudGames.Tests.Infrastructure
{
    public class PurchaseHistoryServiceTests
    {
        [Fact]
        public async Task IndexPurchaseAsync_CallsElasticClient_WithCorrectDocument()
        {
            // Arrange
            var mockClient = new Mock<IElasticClient>();
            var mockLogger = new Mock<ILogger<PurchaseHistoryService>>();

            mockClient
                .Setup(c => c.IndexAsync(It.IsAny<PurchaseHistoryDocument>(), It.IsAny<Func<IndexDescriptor<PurchaseHistoryDocument>, IIndexRequest<PurchaseHistoryDocument>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IndexResponse>());

            var service = new TestablePurchaseHistoryService(mockClient.Object, mockLogger.Object);

            var dto = new EnrichedPurchaseDto
            {
                PurchaseId = Guid.NewGuid(),
                UserCode = 123,
                UserId = Guid.NewGuid(),
                GameCode = 456,
                GameId = Guid.NewGuid(),
                ProcessedAt = DateTimeOffset.UtcNow,
                GameTitle = "Test Game",
                Category = Users.Domain.Enums.GameCategory.Action
            };

            // Act
            await service.IndexPurchaseAsync(dto);

            // Assert
            mockClient.Verify(c => c.IndexAsync(
                It.Is<PurchaseHistoryDocument>(d => d.PurchaseId == dto.PurchaseId && d.UserCode == dto.UserCode && d.GameCode == dto.GameCode && d.Category == dto.Category),
                It.IsAny<Func<IndexDescriptor<PurchaseHistoryDocument>, IIndexRequest<PurchaseHistoryDocument>>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task IndexPurchaseAsync_LogsWarning_WhenResponseInvalid()
        {
            // Arrange
            var mockClient = new Mock<IElasticClient>();
            var mockLogger = new Mock<ILogger<PurchaseHistoryService>>();

            mockClient
                .Setup(c => c.IndexAsync(It.IsAny<PurchaseHistoryDocument>(), It.IsAny<Func<IndexDescriptor<PurchaseHistoryDocument>, IIndexRequest<PurchaseHistoryDocument>>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("index failed"));

            var service = new TestablePurchaseHistoryService(mockClient.Object, mockLogger.Object);

            var dto = new EnrichedPurchaseDto
            {
                PurchaseId = Guid.NewGuid(),
                UserCode = 1,
                UserId = Guid.NewGuid(),
                GameCode = 2,
                GameId = Guid.NewGuid(),
                ProcessedAt = DateTimeOffset.UtcNow,
                GameTitle = "Test Game",
                Category = GameCategory.Casual
            };

            // Act
            await service.IndexPurchaseAsync(dto);

            // Assert
            mockClient.Verify(c => c.IndexAsync(It.IsAny<PurchaseHistoryDocument>(), It.IsAny<Func<IndexDescriptor<PurchaseHistoryDocument>, IIndexRequest<PurchaseHistoryDocument>>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private class TestablePurchaseHistoryService : PurchaseHistoryService
        {
            public TestablePurchaseHistoryService(IElasticClient client, ILogger<PurchaseHistoryService> logger)
                : base(client, logger)
            {
            }

            protected override Task EnsureIndexExistsAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
