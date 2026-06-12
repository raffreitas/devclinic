using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Abstractions;

public interface IPatientService
{
    Task<bool> ExistsAsync(PatientId doctorId, CancellationToken ct = default);
}