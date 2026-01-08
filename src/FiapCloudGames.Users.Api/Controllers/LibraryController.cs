using FiapCloudGames.Users.Api.Extensions;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Users.Api.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento da biblioteca de jogos dos usuários
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class LibraryController : ControllerBase
    {
        private readonly ILibraryService _libraryService;

        public LibraryController(ILibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        /// <summary>
        /// Obtém a biblioteca completa de um usuário
        /// </summary>
        /// <param name="userCode">Código do usuário</param>
        /// <returns>Lista de jogos na biblioteca do usuário</returns>
        /// <response code="200">Retorna a biblioteca do usuário</response>
        /// <response code="404">Usuário não encontrado</response>
        /// <response code="400">Erro na solicitação</response>
        /// <response code="401">Não autorizado</response>
        [HttpGet("user/{userCode}")]
        [Authorize(Roles = "Admin, User")]
        [ProducesResponseType(typeof(IEnumerable<LibraryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserLibrary(int userCode)
        {
            var library = await _libraryService.GetUserLibraryAsync(userCode);
            if (library == null || !library.Any())
            {
                return this.NotFoundProblem("Biblioteca vazia", $"Usuário com código {userCode} não possui jogos na biblioteca.");
            }

            return Ok(LibraryDto.FromEntity(library!));
        }
    }
}
