using System;
using FiapCloudGames.Users.Domain.Enums;

namespace FiapCloudGames.Users.Infrastructure.Elasticsearch
{
    public class PurchaseHistoryDocument
    {
        public Guid PurchaseId { get; set; }
        public int UserCode { get; set; }
        public Guid UserId { get; set; }
        public int GameCode { get; set; }
        public Guid GameId { get; set; }
        public DateTimeOffset ProcessedAt { get; set; }
        public string GameTitle { get; set; } = string.Empty;
        public GameCategory Category { get; set; }
    }
}
