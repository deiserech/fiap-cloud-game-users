using System.Security.Claims;
using FiapCloudGames.Users.Api.Extensions;
using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapCloudGames.Users.Api.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento de usuários
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtém um usuário pelo Código
        /// </summary>
        /// <param name="code">Código do usuário</param>
        /// <returns>Dados do usuário</returns>
        /// <response code="200">Usuário encontrado</response>
        /// <response code="404">Usuário não encontrado</response>
        /// <response code="401">Não autorizado</response>
        [HttpGet("{code}")]
        [Authorize(Roles = "Admin, User")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUser(int code)
        {
            var user = await _service.GetByCodeAsync(code);
            if (user == null)
                return this.NotFoundProblem("Usuário não cadastrado", $"Usuário com código {code} não encontrado.");

            return Ok(UserDto.FromDomainEntity(user));
        }

        /// <summary>
        /// Obtém o perfil do usuário autenticado
        /// </summary>
        /// <returns>Dados do usuário autenticado</returns>
        /// <response code="200">Perfil encontrado</response>
        /// <response code="401">Usuário não autenticado</response>
        /// <response code="404">Usuário não encontrado</response>
        [HttpGet("profile")]
        [Authorize(Roles = "Admin, User")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return this.UnauthorizedProblem("Usuário não autenticado", "Usuário não autenticado ou token ausente.");

            var user = await _service.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
                return this.NotFoundProblem("Erro ao buscar perfil do usuário", "O usuário autenticado não foi encontrado no sistema.");

            return Ok(UserDto.FromDomainEntity(user));
        }

        /// <summary>
        /// Cria um novo usuário
        /// </summary>
        /// <param name="userDto">Dados do usuário a ser criado</param>
        /// <returns>Usuário criado</returns>
        /// <response code="201">Usuário criado com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _service.CreateUserAsync(userDto);

            return CreatedAtAction(nameof(GetUser), new { code = user.Code }, UserDto.FromDomainEntity(user));
        }
    }
}
