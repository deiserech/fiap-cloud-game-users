using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Infrastructure.Services
{
    public class NoopSuggestionService : ISuggestionService
    {
        private readonly ILogger<NoopSuggestionService> _logger;

        public NoopSuggestionService(ILogger<NoopSuggestionService> logger)
        {
            _logger = logger;
        }

        public Task<IEnumerable<GameSuggestionDto>> GetSuggestionsAsync(int userCode)
        {
            _logger.LogInformation("Elasticsearch disabled. Returning empty suggestions. UserCode={UserCode}", userCode);
            return Task.FromResult(Enumerable.Empty<GameSuggestionDto>());
        }
    }
}
