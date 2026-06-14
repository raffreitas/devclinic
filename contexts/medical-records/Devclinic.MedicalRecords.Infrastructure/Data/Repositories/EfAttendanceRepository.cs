using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Infrastructure.Data.Models;
using Devclinic.MedicalRecords.Infrastructure.Data.Serializers;
using Devclinic.MedicalRecords.Infrastructure.Outbox;

using Microsoft.EntityFrameworkCore;

namespace Devclinic.MedicalRecords.Infrastructure.Data.Repositories;

internal sealed class EfAttendanceRepository(
    MedicalRecordsDbContext dbContext,
    AttendanceEventSerializer eventSerializer) : IAttendanceRepository
{
    public async Task<Attendance?> GetByIdAsync(AttendanceId id, CancellationToken cancellationToken)
    {
        var storedEvents = await dbContext.AttendanceEvents
            .AsNoTracking()
            .Where(x => x.AggregateId == id.Value)
            .OrderBy(x => x.Version)
            .ToArrayAsync(cancellationToken);

        if (storedEvents.Length == 0)
            return null;

        var domainEvents = storedEvents
            .Select(x => eventSerializer.Deserialize(
                x.EventType,
                x.Payload))
            .ToArray();

        return Attendance.Reconstitute(domainEvents);
    }

    public async Task SaveAsync(Attendance attendance, CancellationToken cancellationToken)
    {
        var uncommittedEvents = attendance.UnpublishedEvents.ToArray();

        if (uncommittedEvents.Length == 0)
            return;

        var currentVersion = await dbContext.AttendanceEvents
            .Where(x => x.AggregateId == attendance.Id.Value)
            .Select(x => (int?)x.Version)
            .MaxAsync(cancellationToken) ?? 0;

        var version = currentVersion;

        foreach (var domainEvent in uncommittedEvents)
        {
            var serializedEvent = eventSerializer.Serialize(domainEvent);

            dbContext.AttendanceEvents.Add(new StoredAttendanceEvent
            {
                AggregateId = attendance.Id.Value,
                Version = ++version,
                EventType = serializedEvent.EventType,
                Payload = serializedEvent.Payload,
                OccurredAt = DateTime.UtcNow
            });

            var outboxMessage = OutboxMessageFactory.FromDomainEvent(attendance, domainEvent);

            if (outboxMessage is not null)
                dbContext.OutboxMessages.Add(outboxMessage);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        attendance.ClearUnpublishedEvents();
    }
}