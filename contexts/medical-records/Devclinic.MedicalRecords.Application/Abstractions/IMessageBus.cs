namespace Devclinic.MedicalRecords.Application.Abstractions;

public interface IMessageBus
{
    Task PublishAsync(string messageType, string payload, CancellationToken cancellationToken);
}