using System.Collections.ObjectModel;
using System.Text.Json;

using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;
using Devclinic.MedicalRecords.Domain.SeedWork;

namespace Devclinic.MedicalRecords.Infrastructure.Data.Serializers;

internal sealed class MedicalRecordsEventSerializer : IEventSerializer
{
    private static readonly ReadOnlyDictionary<string, Type> EventTypesByName = new Dictionary<string, Type>
    {
        ["medical-record-created.v1"] = typeof(MedicalRecordCreated),
        ["allergy-registered.v1"] = typeof(AllergyRegistered),
        ["medical-record-closed.v1"] = typeof(MedicalRecordClosed)
    }.AsReadOnly();

    private static readonly ReadOnlyDictionary<Type, string> EventNamesByType = EventTypesByName.ToDictionary(
        entry => entry.Value,
        entry => entry.Key).AsReadOnly();

    public SerializedEvent Serialize(IDomainEvent domainEvent)
    {
        var type = domainEvent.GetType();

        if (!EventNamesByType.TryGetValue(type, out var eventType))
            throw new InvalidOperationException($"Unsupported event type: {type.FullName}");

        var payload = JsonSerializer.Serialize(domainEvent, type);

        return new SerializedEvent(eventType, payload);
    }

    public IDomainEvent Deserialize(string eventType, string payload)
    {
        if (!EventTypesByName.TryGetValue(eventType, out var type))
            throw new InvalidOperationException($"Unsupported event type: {eventType}");

        var domainEvent = JsonSerializer.Deserialize(payload, type);

        if (domainEvent is not IDomainEvent typedEvent)
            throw new InvalidOperationException($"Could not deserialize event type: {eventType}");

        return typedEvent;
    }
}