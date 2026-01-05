using System;
using FiapCloudGames.Users.Domain.Enums;

namespace FiapCloudGames.Users.Application.DTOs
{
    public class EnrichedPurchaseDto
    {
        public Guid PurchaseId { get; set; }
        public int UserCode { get; set; }
        public Guid UserId { get; set; }
        public int GameCode { get; set; }
        public Guid GameId { get; set; }
        public string GameTitle { get; set; } = string.Empty;   
        public DateTimeOffset ProcessedAt { get; set; }
        public GameCategory Category { get; set; }
    }
}
