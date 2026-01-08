using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Enums;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Infrastructure.Elasticsearch;
using FiapCloudGames.Users.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using Shouldly;
using Xunit;

namespace FiapCloudGames.Users.Tests.Services;

public class SuggestionServiceTests
{
    private readonly Mock<IElasticClient> _client = new();
    private readonly Mock<IGameRepository> _gameRepository = new();
    private readonly Mock<ILibraryService> _libraryService = new();
    private readonly Mock<ILogger<SuggestionService>> _logger = new();

    private SuggestionService CreateService()
    {
        var configuration = new ConfigurationBuilder().Build();
        return new SuggestionService(_client.Object, _gameRepository.Object, _libraryService.Object, _logger.Object, configuration);
    }

    [Fact]
    public async Task GetSuggestionsAsync_ReturnsEmpty_WhenElasticSearchInvalid()
    {
        // Arrange
        var searchResponse = new Mock<ISearchResponse<PurchaseHistoryDocument>>();
        searchResponse.SetupGet(r => r.IsValid).Returns(false);
        searchResponse.SetupGet(r => r.OriginalException).Returns(new Exception("boom"));

        _client
            .Setup(c => c.SearchAsync<PurchaseHistoryDocument>(It.IsAny<Func<SearchDescriptor<PurchaseHistoryDocument>, ISearchRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        var svc = CreateService();

        // Act
        var result = await svc.GetSuggestionsAsync(10);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetSuggestionsAsync_ReturnsEmpty_WhenNoBuckets()
    {
        // Arrange
        var searchResponse = new Mock<ISearchResponse<PurchaseHistoryDocument>>();
        searchResponse.SetupGet(r => r.IsValid).Returns(true);
        searchResponse
            .SetupGet(r => r.Aggregations)
            .Returns(new AggregateDictionary(new Dictionary<string, IAggregate>()));

        _client
            .Setup(c => c.SearchAsync<PurchaseHistoryDocument>(It.IsAny<Func<SearchDescriptor<PurchaseHistoryDocument>, ISearchRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        var svc = CreateService();

        // Act
        var result = await svc.GetSuggestionsAsync(10);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetSuggestionsAsync_RespectsMaxAndSkipsOwnedGames()
    {
        // Arrange
        var searchResponse = new Mock<ISearchResponse<PurchaseHistoryDocument>>();
        searchResponse.SetupGet(r => r.IsValid).Returns(true);

        var bucket = new KeyedBucket<string>(new Dictionary<string, IAggregate>())
        {
            Key = ((int)GameCategory.Action).ToString()
        };

        var termsAggregate = new TermsAggregate<string>
        {
            Buckets = new List<KeyedBucket<string>> { bucket }
        };

        var dict = new Dictionary<string, IAggregate>
        {
            { "by_category", termsAggregate }
        };

        searchResponse
            .SetupGet(r => r.Aggregations)
            .Returns(new AggregateDictionary(dict));

        _client
            .Setup(c => c.SearchAsync<PurchaseHistoryDocument>(It.IsAny<Func<SearchDescriptor<PurchaseHistoryDocument>, ISearchRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        var ownedGameId = Guid.NewGuid();
        _libraryService
            .Setup(s => s.GetUserLibraryAsync(It.IsAny<int>()))
            .ReturnsAsync(new[] { new Library(Guid.NewGuid(), ownedGameId, Guid.NewGuid(), DateTimeOffset.UtcNow) });

        var g1 = new Game { Id = ownedGameId, Code = 1, Title = "Owned", Category = GameCategory.Action };
        var g2 = new Game { Id = Guid.NewGuid(), Code = 2, Title = "New1", Category = GameCategory.Action };
        var g3 = new Game { Id = Guid.NewGuid(), Code = 3, Title = "New2", Category = GameCategory.Action };

        _gameRepository
            .Setup(r => r.GetByCategoryAsync(GameCategory.Action, It.IsAny<int>()))
            .ReturnsAsync(new[] { g1, g2, g3 });

        var svc = CreateService();

        // Act
        var result = (await svc.GetSuggestionsAsync(10)).ToList();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldAllBe(s => s.GameId != ownedGameId);
    }
}
