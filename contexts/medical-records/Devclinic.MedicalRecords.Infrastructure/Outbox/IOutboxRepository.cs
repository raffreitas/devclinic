namespace Devclinic.MedicalRecords.Infrastructure.Outbox;

internal interface IOutboxRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int limit,
        CancellationToken cancellationToken);

    Task MarkAsProcessedAsync(
        Guid id,
        DateTime processedAt,
        CancellationToken cancellationToken);

    Task MarkAsFailedAsync(
        Guid id,
        string error,
        CancellationToken cancellationToken);
}