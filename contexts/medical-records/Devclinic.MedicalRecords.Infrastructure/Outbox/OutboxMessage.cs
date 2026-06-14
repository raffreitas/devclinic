namespace Devclinic.MedicalRecords.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }

    public Guid EventId { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Payload { get; init; } = string.Empty;

    public DateTime OccurredAt { get; init; }

    public DateTime? ProcessedAt { get; private set; }

    public int Attempts { get; private set; }

    public string? LastError { get; private set; }

    public bool IsProcessed => ProcessedAt is not null;

    public void MarkAsProcessed(DateTime processedAt)
    {
        ProcessedAt = processedAt;
        LastError = null;
    }

    public void MarkAsFailed(string error)
    {
        Attempts++;
        LastError = error;
    }
}