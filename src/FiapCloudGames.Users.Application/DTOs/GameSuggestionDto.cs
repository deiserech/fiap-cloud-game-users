using System;
using FiapCloudGames.Users.Domain.Enums;

namespace FiapCloudGames.Users.Application.DTOs
{
    public class GameSuggestionDto
    {
        public Guid GameId { get; set; }
        public int GameCode { get; set; }
        public string Title { get; set; } = string.Empty;
        public GameCategory Category { get; set; }
    }
}
