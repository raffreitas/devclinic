using Devclinic.MedicalRecords.Application.Abstractions;

using Microsoft.Extensions.Logging;

namespace Devclinic.MedicalRecords.Infrastructure.MessageBus;

internal sealed class LoggingMessageBus(ILogger<LoggingMessageBus> logger) : IMessageBus
{
    public Task PublishAsync(
        string messageType,
        string payload,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Publishing integration message {MessageType}: {Payload}",
            messageType,
            payload);

        return Task.CompletedTask;
    }
}