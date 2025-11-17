using FiapCloudGames.Application.DTOs;

namespace FiapCloudGames.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> Login(LoginDto loginDto);
        Task<AuthResponseDto?> Register(RegisterDto registerDto);
    }
}
