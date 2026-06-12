using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Abstractions;

namespace Devclinic.MedicalRecords.Infrastructure.EventSourcing.Handlers.Common;

internal sealed class ProjectionDispatcher(IEnumerable<IProjectionHandler> handlers)
    : IProjectionDispatcher
{
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var matchingHandlers = handlers
            .Where(x => x.EventType == domainEvent.GetType());

        foreach (var handler in matchingHandlers)
        {
            await handler.HandleAsync(
                domainEvent,
                cancellationToken);
        }
    }
}