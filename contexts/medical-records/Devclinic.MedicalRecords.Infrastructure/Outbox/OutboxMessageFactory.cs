using System.Text.Json;

using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Infrastructure.IntegrationEvents;

namespace Devclinic.MedicalRecords.Infrastructure.Outbox;

internal static class OutboxMessageFactory
{
    public static OutboxMessage? FromDomainEvent(Attendance attendance, IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            AttendanceClosed e => CreateAttendanceClosedMessage(attendance, e),
            _ => null
        };
    }

    private static OutboxMessage CreateAttendanceClosedMessage(
        Attendance attendance,
        AttendanceClosed domainEvent)
    {
        var eventId = Guid.CreateVersion7();

        var integrationEvent = new AttendanceClosedIntegrationEvent(
            EventId: Guid.CreateVersion7(),
            EventType: "attendance-closed.v1",
            OccurredAt: domainEvent.OccurredAt,
            AttendanceId: domainEvent.AttendanceId.Value,
            MedicalRecordId: attendance.MedicalRecordId.Value,
            DoctorId: domainEvent.ClosedBy.Value,
            ClosedAt: domainEvent.OccurredAt);

        return new OutboxMessage
        {
            Id = integrationEvent.EventId,
            EventId = eventId,
            Type = integrationEvent.EventType,
            Payload = JsonSerializer.Serialize(integrationEvent),
            OccurredAt = integrationEvent.OccurredAt
        };
    }
}