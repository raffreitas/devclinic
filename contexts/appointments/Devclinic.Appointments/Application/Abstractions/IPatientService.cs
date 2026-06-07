using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Application.Abstractions;

public interface IPatientService
{
    Task<bool> ExistsAsync(PatientId doctorId, CancellationToken ct = default);
}