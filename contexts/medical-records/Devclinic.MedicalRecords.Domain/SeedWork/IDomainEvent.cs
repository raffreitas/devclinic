namespace Devclinic.MedicalRecords.Domain.SeedWork;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}