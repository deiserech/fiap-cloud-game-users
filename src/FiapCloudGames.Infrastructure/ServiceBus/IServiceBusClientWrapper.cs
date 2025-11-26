using Azure.Messaging.ServiceBus;

namespace FiapCloudGames.Users.Infrastructure.ServiceBus
{
    public interface IServiceBusClientWrapper
    {
        ServiceBusSender GetSender(string queueName);
        ServiceBusProcessor CreateProcessor(string queueName, ServiceBusProcessorOptions? options = null);
        ServiceBusProcessor CreateProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions? options = null);
    }
}
