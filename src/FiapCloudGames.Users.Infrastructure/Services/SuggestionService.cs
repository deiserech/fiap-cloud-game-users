using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Enums;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Infrastructure.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;

namespace FiapCloudGames.Users.Infrastructure.Services
{
    public class SuggestionService : ISuggestionService
    {
        private const string PurchaseHistoryIndex = "purchases-history";
        private const string CategoryAggregationName = "by_category";
        private const string PopularGamesAggregationName = "by_game";

        private readonly IElasticClient _client;
        private readonly IGameRepository _gameRepository;
        private readonly ILibraryService _libraryService;
        private readonly ILogger<SuggestionService> _logger;
        private readonly int _maxSuggestions;

        public SuggestionService(IElasticClient client, IGameRepository gameRepository, ILibraryService libraryService, ILogger<SuggestionService> logger, IConfiguration configuration)
        {
            _client = client;
            _gameRepository = gameRepository;
            _libraryService = libraryService;
            _logger = logger;
            _maxSuggestions = int.TryParse(configuration["SUGGESTIONS_MAX_GAMES"], out var parsed) && parsed > 0
                ? parsed
                : 3;
        }

        public async Task<IEnumerable<GameSuggestionDto>> GetSuggestionsAsync(int userCode)
        {
            try
            {
                var categoryBuckets = await GetUserCategoryBucketsAsync(userCode);
                if (categoryBuckets == null || categoryBuckets.Count == 0)
                {
                    _logger.LogInformation("Elasticsearch suggestion empty.");
                    return [];
                }

                var ownedCodes = await GetOwnedGameCodesAsync(userCode);
                var suggestions = new List<GameSuggestionDto>();

                await AddCategorySuggestionsAsync(categoryBuckets, ownedCodes, suggestions, _maxSuggestions);

                if (suggestions.Count < _maxSuggestions)
                {
                    var blockedCodes = new HashSet<int>(ownedCodes);
                    blockedCodes.UnionWith(suggestions.Select(s => s.GameCode));
                    await AddPopularSuggestionsAsync(blockedCodes, suggestions, _maxSuggestions);
                }

                _logger.LogDebug("Suggestions built. Total: {Count}, Suggestions: {@Suggestions}", suggestions.Count, suggestions);
                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building suggestions for user {UserCode}", userCode);
                return [];
            }
        }

        private async Task<IReadOnlyCollection<KeyedBucket<string>>?> GetUserCategoryBucketsAsync(int userCode)
        {
            var resp = await _client.SearchAsync<PurchaseHistoryDocument>(s => s
                .Index(PurchaseHistoryIndex)
                .Size(0)
                .Query(q => q.Bool(b => b.Must(
                    mu => mu.Term(t => t.Field(f => f.UserCode).Value(userCode))
                )))
                .Aggregations(a => a.Terms(CategoryAggregationName, t => t.Field(f => f.Category).Size(10).Order(o => o.Descending("_count"))))
            );

            if (!resp.IsValid)
            {
                _logger.LogWarning("Elasticsearch suggestion query invalid: {Reason}", resp.OriginalException?.Message ?? resp.DebugInformation);
                return null;
            }

            return resp.Aggregations.Terms(CategoryAggregationName)?.Buckets;
        }

        private async Task<HashSet<int>> GetOwnedGameCodesAsync(int userCode)
        {
            var library = await _libraryService.GetUserLibraryAsync(userCode);
            return [.. library.Select(l => l.Game.Code)];
        }

        private async Task AddCategorySuggestionsAsync(
            IReadOnlyCollection<KeyedBucket<string>> buckets,
            HashSet<int> ownedGameCodes,
            List<GameSuggestionDto> suggestions,
            int max)
        {
            foreach (var bucket in buckets)
            {
                if (suggestions.Count >= max) break;
                if (!int.TryParse(bucket.Key, out var catInt)) continue;

                var category = (GameCategory)catInt;
                var remaining = max - suggestions.Count;
                var games = await GetGamesByCategoryFromElasticAsync(category, remaining);

                foreach (var game in games)
                {
                    if (ownedGameCodes.Contains(game.Code)) continue;

                    suggestions.Add(MapToSuggestion(game));
                    if (suggestions.Count >= max) break;
                }
            }

        }

        private async Task AddPopularSuggestionsAsync(
            HashSet<int> blockedGameCodes,
            List<GameSuggestionDto> suggestions,
            int max)
        {
            var popularResp = await _client.SearchAsync<PurchaseHistoryDocument>(s => s
                .Index(PurchaseHistoryIndex)
                .Size(0)
                .Aggregations(a => a.Terms(PopularGamesAggregationName, t => t.Field(f => f.GameCode).Size(50).Order(o => o.Descending("_count"))))
            );

            if (!popularResp.IsValid)
            {
                _logger.LogWarning("Elasticsearch popular games query invalid: {Reason}", popularResp.OriginalException?.Message ?? popularResp.DebugInformation);
                return;
            }

            var gameBuckets = popularResp.Aggregations.Terms(PopularGamesAggregationName)?.Buckets;
            if (gameBuckets == null)
            {
                return;
            }

            foreach (var bucket in gameBuckets)
            {
                if (suggestions.Count >= max) break;
                if (!int.TryParse(bucket.Key, out var gameCode)) continue;
                if (blockedGameCodes.Contains(gameCode)) continue;

                var game = await GetGameByCodeFromElasticAsync(gameCode);
                if (game == null) continue;

                suggestions.Add(MapToSuggestion(game));
                blockedGameCodes.Add(game.Code);
            }
        }

        private async Task<Game?> GetGameByCodeFromElasticAsync(int gameCode)
        {
            var response = await _client.SearchAsync<Game>(s => s
                .Index("games")
                .Size(1)
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            mu => mu.Term(t => t.Field(f => f.Code).Value(gameCode))
                        )
                    )
                )
            );

            if (!response.IsValid)
            {
                _logger.LogWarning("Elasticsearch game by code query invalid: {Reason}", response.OriginalException?.Message ?? response.DebugInformation);
                return null;
            }

            return response.Documents.FirstOrDefault();
        }

        private static GameSuggestionDto MapToSuggestion(Game game) => new()
        {
            GameId = game.Id,
            GameCode = game.Code,
            Title = game.Title,
            Category = game.Category
        };
        private async Task<IEnumerable<Game>> GetGamesByCategoryFromElasticAsync(GameCategory category, int limit)
        {
            var response = await _client.SearchAsync<Game>(s => s
                .Index("games") 
                .Size(limit)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.Category)
                        .Value((int)category)
                    )
                )
            );

            if (!response.IsValid)
            {
                _logger.LogWarning("Elasticsearch game category query invalid: {Reason}", response.OriginalException?.Message ?? response.DebugInformation);
                return [];
            }

            return response.Documents;
        }

    }

}
