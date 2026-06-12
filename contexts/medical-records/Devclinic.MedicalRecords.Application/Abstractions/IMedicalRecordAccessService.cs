using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Abstractions;

public interface IMedicalRecordAccessService
{
    Task<bool> CanWriteAsync(DoctorId doctorId, PatientId patientId, CancellationToken cancellationToken);
}