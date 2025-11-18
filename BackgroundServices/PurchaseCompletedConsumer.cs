using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using FiapCloudGames.Users.Services;
using FiapCloudGames.Users.Data;
using FiapCloudGames.Users.Models;

namespace FiapCloudGames.Users.BackgroundServices;

public class PurchaseCompletedConsumer : BackgroundService
{
    private readonly ServiceBusClientWrapper _sb;
    private readonly IServiceProvider _provider;
    private ServiceBusProcessor? _processor;
    private readonly IConfiguration _config;

    public PurchaseCompletedConsumer(ServiceBusClientWrapper sb, IServiceProvider provider, IConfiguration config)
    {
        _sb = sb;
        _provider = provider;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = _config["SERVICE_BUS_QUEUE_COMPLETED"] ?? "payments/purchases-completed";
        _processor = _sb.CreateProcessor(queue);
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ErrorHandler;
        await _processor.StartProcessingAsync(stoppingToken);
    }

    private Task ErrorHandler(ProcessErrorEventArgs arg)
    {
        Console.WriteLine($"PurchaseCompletedConsumer error: {arg.Exception}");
        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        var msg = JsonConvert.DeserializeObject<PurchaseCompletedMessage>(body);
        if (msg == null)
        {
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Idempotency: check if library entry already exists for this purchaseId
        var exists = db.Library.Any(l => l.PurchaseId == msg.PurchaseId);
        if (exists)
        {
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        var entry = new LibraryEntry
        {
            Id = Guid.NewGuid(),
            UserId = msg.UserId,
            GameId = msg.GameId,
            PurchaseId = msg.PurchaseId,
            AcquiredAt = msg.ProcessedAt
        };

        db.Library.Add(entry);
        await db.SaveChangesAsync();

        await args.CompleteMessageAsync(args.Message);
    }
}

public record PurchaseCompletedMessage(Guid PurchaseId, Guid UserId, Guid GameId, decimal Amount, string Currency, DateTimeOffset ProcessedAt, Guid QuoteId);
