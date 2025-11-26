using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace FiapCloudGames.Users.Infrastructure.ServiceBus
{
    public class ServiceBusClientWrapper : IServiceBusClientWrapper
    {
        private readonly ServiceBusClient _client;

        public ServiceBusClientWrapper(IOptions<ServiceBusOptions> options)
        {
            var conn = options?.Value?.ConnectionString ?? throw new InvalidOperationException("ServiceBus:ConnectionString is required");
            _client = new ServiceBusClient(conn);
        }

        public ServiceBusSender GetSender(string queueName) => _client.CreateSender(queueName);

        public ServiceBusProcessor CreateProcessor(string queueName, ServiceBusProcessorOptions? options = null)
            => _client.CreateProcessor(queueName, options ?? new ServiceBusProcessorOptions { MaxConcurrentCalls = 1, AutoCompleteMessages = false });
    }
}
