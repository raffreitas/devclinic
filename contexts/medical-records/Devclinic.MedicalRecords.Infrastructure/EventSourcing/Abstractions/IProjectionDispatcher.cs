using Devclinic.MedicalRecords.Domain.SeedWork;

namespace Devclinic.MedicalRecords.Infrastructure.EventSourcing.Abstractions;

public interface IProjectionDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}