namespace FiapCloudGames.Users.Domain.Events
{
    public record PurchaseCompletedEvent(
        Guid PurchaseId,
        Guid UserId,
        Guid GameId,
        decimal Amount,
        string Currency,
        DateTimeOffset ProcessedAt,
        Guid QuoteId);
}
