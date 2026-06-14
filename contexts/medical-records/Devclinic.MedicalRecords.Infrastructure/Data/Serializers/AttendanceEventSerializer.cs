using System.Collections.ObjectModel;
using System.Text.Json;

using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;
using Devclinic.MedicalRecords.Domain.SeedWork;

namespace Devclinic.MedicalRecords.Infrastructure.Data.Serializers;

internal sealed class AttendanceEventSerializer : IEventSerializer
{
    private static readonly IReadOnlyDictionary<string, Type> EventTypesByName = new Dictionary<string, Type>
    {
        ["attendance-started.v1"] = typeof(AttendanceStarted),
        ["chief-complaint-registered.v1"] = typeof(ChiefComplaintRegistered),
        ["diagnosis-registered.v1"] = typeof(DiagnosisRegistered),
        ["prescription-issued.v1"] = typeof(PrescriptionIssued),
        ["attendance-closed.v1"] = typeof(AttendanceClosed)
    };

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