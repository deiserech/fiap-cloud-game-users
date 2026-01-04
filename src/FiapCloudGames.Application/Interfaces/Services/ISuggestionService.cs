using FiapCloudGames.Users.Application.DTOs;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface ISuggestionService
    {
        Task<IEnumerable<GameSuggestionDto>> GetSuggestionsAsync(int userCode);
    }
}
