using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;
using Devclinic.MedicalRecords.Infrastructure.Data.Models;
using Devclinic.MedicalRecords.Infrastructure.Data.Serializers;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace Devclinic.MedicalRecords.Infrastructure.Data.Repositories;

internal sealed class EfMedicalRecordRepository(
    MedicalRecordsDbContext dbContext,
    MedicalRecordsEventSerializer eventSerializer,
    IProjectionDispatcher projectionDispatcher) : IMedicalRecordRepository
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

    public async Task<bool> ExistsByPatientIdAsync(PatientId patientId,
        CancellationToken cancellationToken)
    {
        return await dbContext.MedicalRecordIndexes
            .AsNoTracking()
            .AnyAsync(x => x.PatientId == patientId.Value
                           && x.Status == nameof(MedicalRecordStatus.Active), cancellationToken);
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

        foreach (var domainEvent in uncommittedEvents)
        {
            var serializedEvent = eventSerializer.Serialize(domainEvent);

            dbContext.MedicalRecordEvents.Add(new StoredMedicalRecordEvent
            {
                AggregateId = medicalRecord.Id.Value,
                Version = ++version,
                EventType = serializedEvent.EventType,
                Payload = serializedEvent.Payload,
                OccurredAt = DateTime.UtcNow
            });

            await projectionDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        medicalRecord.ClearUnpublishedEvents();
    }
}