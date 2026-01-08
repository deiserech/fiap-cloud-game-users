using FiapCloudGames.Users.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FiapCloudGames.Users.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class SuggestionsController : ControllerBase
    {
        private readonly ISuggestionService _suggestionService;

        public SuggestionsController(ISuggestionService suggestionService)
        {
            _suggestionService = suggestionService;
        }

        [HttpGet("user/{userCode}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetSuggestions(int userCode)
        {
            var suggestions = await _suggestionService.GetSuggestionsAsync(userCode);
            if (suggestions == null || !suggestions.Any())
            {
                return NotFound();
            }

            return Ok(suggestions);
        }
    }
}
