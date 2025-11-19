using FiapCloudGames.Users.Application.DTOs;
using FiapCloudGames.Users.Application.Services;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<ILogger<GameService>> _mockLogger;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _mockGameRepository = new Mock<IGameRepository>();
            _mockLogger = new Mock<ILogger<GameService>>();
            _gameService = new GameService(_mockGameRepository.Object, _mockLogger.Object);
        }


    }
}
