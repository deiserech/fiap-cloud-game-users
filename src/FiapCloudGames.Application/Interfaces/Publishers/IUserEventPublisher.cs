using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Application.Interfaces.Publishers;
public interface IUserEventPublisher
{
    Task PublishUserEventAsync(User user, bool isRemoved = false);
}
