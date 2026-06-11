using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(MedicalRecordId id, CancellationToken cancellationToken);

    Task SaveAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken);
}