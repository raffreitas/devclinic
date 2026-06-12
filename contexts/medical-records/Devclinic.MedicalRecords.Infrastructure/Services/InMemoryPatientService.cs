using Devclinic.MedicalRecords.Application.Abstractions;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Infrastructure.Services;

internal sealed class InMemoryPatientService : IPatientService
{
    public Task<bool> ExistsAsync(PatientId doctorId, CancellationToken ct = default) =>
        Task.FromResult(doctorId.Value != Guid.Empty);
}