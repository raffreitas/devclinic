using Devclinic.MedicalRecords.Application.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Devclinic.MedicalRecords.Infrastructure.Outbox;

internal sealed class PublishOutboxMessagesWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<PublishOutboxMessagesWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishPendingMessagesAsync(stoppingToken);

            await Task.Delay(Delay, stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var messages = await outboxRepository.GetPendingAsync(
            limit: 50,
            cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await messageBus.PublishAsync(
                    message.Type,
                    message.Payload,
                    cancellationToken);

                await outboxRepository.MarkAsProcessedAsync(
                    message.Id,
                    DateTime.UtcNow,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to publish outbox message {OutboxMessageId}",
                    message.Id);

                await outboxRepository.MarkAsFailedAsync(
                    message.Id,
                    exception.Message,
                    cancellationToken);
            }
        }
    }
}