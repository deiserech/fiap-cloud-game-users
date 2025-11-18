using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace FiapCloudGames.Users.Infrastructure.ServiceBus;

public class ServiceBusClientWrapper
{
    private readonly ServiceBusClient _client;
    private readonly IConfiguration _config;

    public ServiceBusClientWrapper(IConfiguration config)
    {
        _config = config;
        var conn = config["SERVICE_BUS_CONNECTION_STRING"] ?? throw new InvalidOperationException("SERVICE_BUS_CONNECTION_STRING is required");
        _client = new ServiceBusClient(conn);
    }

    public ServiceBusSender GetSender(string queueName) => _client.CreateSender(queueName);
    public ServiceBusProcessor CreateProcessor(string queueName, ServiceBusProcessorOptions? options = null)
        => _client.CreateProcessor(queueName, options ?? new ServiceBusProcessorOptions { MaxConcurrentCalls = 1, AutoCompleteMessages = false });
}
