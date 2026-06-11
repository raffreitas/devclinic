using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Infrastructure.Services;

internal sealed class InMemoryDoctorService : IDoctorService
{
    public Task<bool> ExistsAsync(DoctorId doctorId, CancellationToken ct = default) =>
        Task.FromResult(doctorId.Value != Guid.Empty);
}