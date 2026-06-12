using Devclinic.MedicalRecords.Application.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Features.CreateMedicalRecord;

public sealed record CreateMedicalRecordCommand(Guid PatientId);

public sealed class CreateMedicalRecordUseCase(
    IPatientService patientService,
    IMedicalRecordRepository medicalRecordRepository)
{
    public async Task<MedicalRecordId> ExecuteAsync(CreateMedicalRecordCommand command, CancellationToken ct = default)
    {
        var patientId = new PatientId(command.PatientId);

        var patientExists = await patientService.ExistsAsync(patientId, ct);
        if (!patientExists)
            throw new InvalidOperationException("The patient does not exist.");

        var medicalRecordExists = await medicalRecordRepository.ExistsByPatientIdAsync(patientId, ct);
        if (medicalRecordExists)
            throw new InvalidOperationException("The medical record already exists.");

        var medicalRecordId = new MedicalRecordId(Guid.CreateVersion7());
        var medicalRecord = MedicalRecord.Create(medicalRecordId, patientId);

        await medicalRecordRepository.SaveAsync(medicalRecord, ct);

        return medicalRecord.Id;
    }
}