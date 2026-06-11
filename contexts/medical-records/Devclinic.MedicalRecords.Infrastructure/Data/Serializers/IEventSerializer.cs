using Devclinic.MedicalRecords.Domain.SeedWork;

namespace Devclinic.MedicalRecords.Infrastructure.Data.Serializers;

public interface IEventSerializer
{
    SerializedEvent Serialize(IDomainEvent domainEvent);

    IDomainEvent Deserialize(string eventType, string payload);
}

public sealed record SerializedEvent(string EventType, string Payload);