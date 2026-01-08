using FiapCloudGames.Users.Application.Interfaces.Publishers;
using FiapCloudGames.Users.Domain.Entities;
using FiapCloudGames.Users.Domain.Events;
using FiapCloudGames.Users.Infrastructure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Users.Infrastructure.Publishers
{
    public class UserEventPublisher : IUserEventPublisher
    {
        private readonly IServiceBusPublisher _busPublisher;
        private readonly ILogger<UserEventPublisher> _logger;
        private const string UserTopic = "users-created-updated";

        public UserEventPublisher(IServiceBusPublisher busPublisher, ILogger<UserEventPublisher> logger)
        {
            _busPublisher = busPublisher;
            _logger = logger;
        }

        public async Task PublishUserEventAsync(User user, bool isRemoved = false)
        {
            DateTimeOffset? removedAt = isRemoved ? DateTimeOffset.UtcNow : null;
            var evt = new UserEvent(user.Code, user.Email, DateTimeOffset.Now, removedAt);
            try
            {
                await _busPublisher.PublishAsync(evt, UserTopic);
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao publicar evento {Evento}: {UserCode}. Message: {Message}", nameof(UserEvent), user.Code, e.Message);
            }
        }
    }
}
