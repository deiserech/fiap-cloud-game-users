using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Api.Controllers
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
        /// <param name="userId">ID do usuário</param>
        /// <returns>Lista de jogos na biblioteca do usuário</returns>
        /// <response code="200">Retorna a biblioteca do usuário</response>
        /// <response code="404">Usuário não encontrado</response>
        /// <response code="400">Erro na solicitação</response>
        /// <response code="401">Não autorizado</response>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin, User")]
        [ProducesResponseType(typeof(IEnumerable<Library>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<Library>>> GetUserLibrary(int userId)
        {
            try
            {
                var library = await _libraryService.GetUserLibraryAsync(userId);
                return Ok(library);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtém uma entrada específica da biblioteca pelo ID
        /// </summary>
        /// <param name="id">ID da entrada da biblioteca</param>
        /// <returns>Dados da entrada da biblioteca</returns>
        /// <response code="200">Retorna a entrada da biblioteca</response>
        /// <response code="404">Entrada não encontrada</response>
        /// <response code="400">Erro na solicitação</response>
        /// <response code="401">Não autorizado</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, User")]
        [ProducesResponseType(typeof(Library), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Library>> GetLibraryEntry(int id)
        {
            try
            {
                var entry = await _libraryService.GetLibraryEntryAsync(id);
                if (entry == null)
                {
                    return NotFound();
                }
                return Ok(entry);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
