using Devclinic.MedicalRecords.Domain.SeedWork;

namespace Devclinic.MedicalRecords.Infrastructure.EventSourcing.Abstractions;

public interface IProjectionHandler
{
    Type EventType { get; }

    Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}

public interface IProjectionHandler<in TEvent> : IProjectionHandler
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}