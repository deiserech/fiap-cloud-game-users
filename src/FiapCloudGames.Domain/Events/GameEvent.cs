namespace FiapCloudGames.Users.Domain.Entities.Events
{
    public record GameEvent(
        Guid Id,
        int Code,
        string Title,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? RemovedAt);
}
