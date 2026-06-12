using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Abstractions;

namespace Devclinic.MedicalRecords.Infrastructure.EventSourcing.Handlers.Common;

internal abstract class ProjectionHandler<TEvent> : IProjectionHandler<TEvent> where TEvent : IDomainEvent
{
    public Type EventType => typeof(TEvent);

    public abstract Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);

    public Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        return HandleAsync((TEvent)domainEvent, cancellationToken);
    }
}