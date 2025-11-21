namespace FiapCloudGames.Users.Domain.Events
{
    public record PurchaseCompletedEvent(
        Guid PurchaseId,
        int UserCode,
        int GameCode,
        DateTimeOffset ProcessedAt,
        bool Success);
}
