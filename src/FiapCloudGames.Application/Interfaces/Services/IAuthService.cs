using FiapCloudGames.Users.Application.DTOs;

namespace FiapCloudGames.Users.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> Login(LoginDto loginDto);
        Task<AuthResponseDto?> Register(RegisterDto registerDto);
    }
}
