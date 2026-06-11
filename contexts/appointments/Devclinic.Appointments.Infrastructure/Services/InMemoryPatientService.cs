using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Infrastructure.Services;

internal sealed class InMemoryPatientService : IPatientService
{
    public Task<bool> ExistsAsync(PatientId doctorId, CancellationToken ct = default) =>
        Task.FromResult(doctorId.Value != Guid.Empty);
}