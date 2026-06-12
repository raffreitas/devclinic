using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(MedicalRecordId id, CancellationToken cancellationToken);

    Task<bool> ExistsByPatientIdAsync(PatientId patientId, CancellationToken cancellationToken);

    Task SaveAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken);
}