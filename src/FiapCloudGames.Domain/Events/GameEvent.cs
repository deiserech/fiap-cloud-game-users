using FiapCloudGames.Users.Domain.Enums;

namespace FiapCloudGames.Users.Domain.Entities.Events
{
    public record GameEvent(
        Guid Id,
        int Code,
        string Title,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? RemovedAt,
        GameCategory Category = GameCategory.Unknown);
}
