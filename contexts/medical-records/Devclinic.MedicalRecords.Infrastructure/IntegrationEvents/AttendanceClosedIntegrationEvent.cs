namespace Devclinic.MedicalRecords.Infrastructure.IntegrationEvents;

public sealed record AttendanceClosedIntegrationEvent(
    Guid EventId,
    string EventType,
    DateTime OccurredAt,
    Guid AttendanceId,
    Guid MedicalRecordId,
    Guid DoctorId,
    DateTime ClosedAt);