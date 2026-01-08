namespace FiapCloudGames.Users.Domain.Events
{
    public record UserEvent(
    int Code,
    string Email,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RemovedAt);
}
