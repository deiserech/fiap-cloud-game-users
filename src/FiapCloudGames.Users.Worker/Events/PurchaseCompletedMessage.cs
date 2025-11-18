namespace FiapCloudGames.Users.Worker.Events
{
    public record PurchaseCompletedMessage(
        Guid PurchaseId,
        Guid UserId,
        Guid GameId,
        decimal Amount,
        string Currency,
        DateTimeOffset ProcessedAt,
        Guid QuoteId);
}
