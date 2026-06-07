using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Application.Abstractions;

public interface IDoctorService
{
    Task<bool> ExistsAsync(DoctorId doctorId, CancellationToken ct = default);
}