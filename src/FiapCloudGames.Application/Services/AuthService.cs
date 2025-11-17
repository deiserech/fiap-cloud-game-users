using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Domain.Interfaces.Repositories;
using FiapCloudGames.Application.Interfaces.Services;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Shared.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FiapCloudGames.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly Domain.Interfaces.Repositories.IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(Domain.Interfaces.Repositories.IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> Login(LoginDto loginDto)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(AuthService)}.Login");
            _logger.LogInformation("Tentativa de login para o email: {Email}", loginDto.Email);
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            if (user == null || !user.VerifyPassword(loginDto.Password))
            {
                _logger.LogWarning("Falha no login para o email: {Email}", loginDto.Email);
                return null;
            }

            var token = GenerateJwtToken(user);

            _logger.LogInformation("Login realizado com sucesso para o email: {Email}", loginDto.Email);
            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                UserId = user.Id
            };
        }

        public async Task<AuthResponseDto?> Register(RegisterDto registerDto)
        {
            using var activity = Tracing.ActivitySource.StartActivity($"{nameof(AuthService)}.Register");
            _logger.LogInformation("Tentativa de registro para o email: {Email}", registerDto.Email);
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                _logger.LogWarning("Registro falhou: email já existe: {Email}", registerDto.Email);
                return null;
            }

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Role = registerDto.Role
            };

            user.SetPassword(registerDto.Password);

            await _userRepository.CreateAsync(user);

            var token = GenerateJwtToken(user);

            _logger.LogInformation("Usuário registrado com sucesso: {Email}", registerDto.Email);
            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                UserId = user.Id
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");

            if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
            {
                throw new InvalidOperationException("JWT SecretKey is missing or too short. It must be at least 32 characters long.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
