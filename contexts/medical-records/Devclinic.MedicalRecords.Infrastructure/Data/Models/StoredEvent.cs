namespace Devclinic.MedicalRecords.Infrastructure.Data.Models;

public abstract class StoredEvent
{
    public long Id { get; init; }
    public Guid AggregateId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public int Version { get; init; }
}

public sealed class StoredMedicalRecordEvent : StoredEvent;

public sealed class StoredAttendanceEvent : StoredEvent;