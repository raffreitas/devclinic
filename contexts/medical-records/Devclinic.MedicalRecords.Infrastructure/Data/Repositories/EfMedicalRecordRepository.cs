using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Infrastructure.Data.Models;
using Devclinic.MedicalRecords.Infrastructure.Data.Serializers;

using Microsoft.EntityFrameworkCore;

namespace Devclinic.MedicalRecords.Infrastructure.Data.Repositories;

internal sealed class EfMedicalRecordRepository(
    MedicalRecordsDbContext dbContext,
    MedicalRecordsEventSerializer eventSerializer) : IMedicalRecordRepository
{
    public async Task<MedicalRecord?> GetByIdAsync(MedicalRecordId id, CancellationToken cancellationToken)
    {
        var storedEvents = await dbContext.MedicalRecordEvents
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

        return MedicalRecord.Reconstitute(domainEvents);
    }

    public async Task SaveAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken)
    {
        var uncommittedEvents = medicalRecord.UnpublishedEvents.ToArray();

        if (uncommittedEvents.Length == 0)
            return;

        var currentVersion = await dbContext.MedicalRecordEvents
            .Where(x => x.AggregateId == medicalRecord.Id.Value)
            .Select(x => (int?)x.Version)
            .MaxAsync(cancellationToken) ?? 0;

        var version = currentVersion;

        var storedEvents = uncommittedEvents
            .Select(domainEvent =>
            {
                version++;

                var serializedEvent =
                    eventSerializer.Serialize(domainEvent);

                return new StoredMedicalRecordEvent
                {
                    AggregateId = medicalRecord.Id.Value,
                    Version = version,
                    EventType = serializedEvent.EventType,
                    Payload = serializedEvent.Payload,
                    OccurredAt = DateTime.UtcNow
                };
            })
            .ToArray();

        dbContext.MedicalRecordEvents.AddRange(storedEvents);
        await dbContext.SaveChangesAsync(cancellationToken);
        medicalRecord.ClearUnpublishedEvents();
    }
}