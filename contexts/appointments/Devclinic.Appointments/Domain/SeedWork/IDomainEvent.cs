namespace Devclinic.Appointments.Domain.SeedWork;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}