using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;
using Devclinic.MedicalRecords.Infrastructure.Data;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Handlers.Common;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Projections;

namespace Devclinic.MedicalRecords.Infrastructure.EventSourcing.Handlers;

internal sealed class MedicalRecordCreatedProjectionHandler(MedicalRecordsDbContext dbContext)
    : ProjectionHandler<MedicalRecordCreated>
{
    public override Task HandleAsync(MedicalRecordCreated domainEvent, CancellationToken cancellationToken = default)
    {
        dbContext.MedicalRecordIndexes.Add(new MedicalRecordIndex
        {
            MedicalRecordId = domainEvent.MedicalRecordId.Value,
            PatientId = domainEvent.PatientId.Value,
            Status = nameof(MedicalRecordStatus.Active)
        });

        return Task.CompletedTask;
    }
}