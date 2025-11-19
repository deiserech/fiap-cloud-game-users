namespace FiapCloudGames.Users.Domain.Entities
{
    public class Library
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid GameId { get; set; }

        public User User { get; set; } = null!;

        public Game Game { get; set; } = null!;

        public DateTimeOffset AcquiredAt { get; set; }

        public Guid PurchaseId { get; set; }

        public Library(Guid userId, Guid gameId, Guid purchaseId, DateTimeOffset acquiredAt)
        {
            UserId = userId;
            GameId = gameId;
            PurchaseId = purchaseId;
            AcquiredAt = acquiredAt;
        }
    }
}

