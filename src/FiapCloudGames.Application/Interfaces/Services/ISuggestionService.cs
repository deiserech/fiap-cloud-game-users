using System.Collections.Generic;
using System.Threading.Tasks;
using FiapCloudGames.Users.Application.DTOs;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface ISuggestionService
    {
        Task<IEnumerable<GameSuggestionDto>> GetSuggestionsAsync(int userCode, int max = 3);
    }
}
