using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using FiapCloudGames.Users.Domain.Enums;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FiapCloudGames.Users.Infrastructure.Elasticsearch;
using Microsoft.Extensions.Logging;
using Nest;

namespace FiapCloudGames.Users.Infrastructure.Services
{
    public class SuggestionService : ISuggestionService
    {
        private readonly IElasticClient _client;
        private readonly IGameRepository _gameRepository;
        private readonly ILibraryService _libraryService;
        private readonly ILogger<SuggestionService> _logger;

        public SuggestionService(IElasticClient client, IGameRepository gameRepository, ILibraryService libraryService, ILogger<SuggestionService> logger)
        {
            _client = client;
            _gameRepository = gameRepository;
            _libraryService = libraryService;
            _logger = logger;
        }

        public async Task<IEnumerable<GameSuggestionDto>> GetSuggestionsAsync(int userCode, int max = 3)
        {
            try
            {
                var resp = await _client.SearchAsync<PurchaseHistoryDocument>(s => s
                    .Index("purchases-history")
                    .Size(0)
                    .Query(q => q.Bool(b => b.Must(
                        mu => mu.Term(t => t.Field(f => f.UserCode).Value(userCode)),
                        mu => mu.Term(t => t.Field(f => f.Success).Value(true))
                    )))
                    .Aggregations(a => a.Terms("by_category", t => t.Field(f => f.Category).Size(10).Order(o => o.Descending("_count"))))
                );

                if (!resp.IsValid)
                {
                    _logger.LogWarning("Elasticsearch suggestion query invalid: {Reason}", resp.OriginalException?.Message ?? resp.DebugInformation);
                    return [];
                }

                var terms = resp.Aggregations.Terms("by_category")?.Buckets;
                if (terms == null || terms.Count == 0)
                {

                    _logger.LogInformation("Elasticsearch suggestion empty.");
                    return [];
                }

                var library = await _libraryService.GetUserLibraryAsync(userCode);
                var ownedIds = new HashSet<Guid>(library.Select(l => l.GameId));

                var suggestions = new List<GameSuggestionDto>();
                foreach (var bucket in terms)
                {
                    if (suggestions.Count >= max) break;
                    if (!int.TryParse(bucket.Key, out int catInt)) continue;
                    var category = (GameCategory)catInt;
                    var games = await _gameRepository.GetByCategoryAsync(category, max - suggestions.Count);
                    foreach (var g in games)
                    {
                        if (ownedIds.Contains(g.Id)) continue;
                        suggestions.Add(new GameSuggestionDto
                        {
                            GameId = g.Id,
                            GameCode = g.Code,
                            Title = g.Title,
                            Category = g.Category
                        });
                        if (suggestions.Count >= max) break;
                    }
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
    }
}
