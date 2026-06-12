using Devclinic.MedicalRecords.Application.Abstractions;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Infrastructure.Services;

internal sealed class InMemoryMedicalRecordAccessService : IMedicalRecordAccessService
{
    public Task<bool> CanWriteAsync(DoctorId doctorId, PatientId patientId, CancellationToken cancellationToken)
    {
        // For demonstration purposes, this implementation allows all access.
        // In a real application, you would check the doctor's permissions against the patient's medical record.
        return Task.FromResult(true);
    }
}