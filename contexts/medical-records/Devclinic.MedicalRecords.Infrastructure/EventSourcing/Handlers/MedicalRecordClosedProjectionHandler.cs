using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;
using Devclinic.MedicalRecords.Infrastructure.Data;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Handlers.Common;

using Microsoft.EntityFrameworkCore;

namespace Devclinic.MedicalRecords.Infrastructure.EventSourcing.Handlers;

internal sealed class MedicalRecordClosedProjectionHandler(MedicalRecordsDbContext dbContext)
    : ProjectionHandler<MedicalRecordClosed>
{
    public override async Task HandleAsync(MedicalRecordClosed domainEvent, CancellationToken cancellationToken)
    {
        var index = await dbContext.MedicalRecordIndexes
            .SingleAsync(x => x.MedicalRecordId == domainEvent.MedicalRecordId.Value, cancellationToken);

        index.Status = nameof(MedicalRecordStatus.Closed);
    }
}