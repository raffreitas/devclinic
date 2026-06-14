using Devclinic.MedicalRecords.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace Devclinic.MedicalRecords.Infrastructure.Outbox;

internal sealed class EfOutboxRepository(MedicalRecordsDbContext dbContext) : IOutboxRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        return await dbContext.OutboxMessages
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.OccurredAt)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(
        Guid id,
        DateTime processedAt,
        CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages
            .SingleAsync(x => x.Id == id, cancellationToken);

        message.MarkAsProcessed(processedAt);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsFailedAsync(
        Guid id,
        string error,
        CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages
            .SingleAsync(x => x.Id == id, cancellationToken);

        message.MarkAsFailed(error);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}