using Devclinic.MedicalRecords.Application.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Features.RegisterAllergy;

public sealed record RegisterAllergyCommand(
    Guid MedicalRecordId,
    string Substance,
    string Severity);

public sealed class RegisterAllergyUseCase(
    IMedicalRecordRepository medicalRecordRepository,
    IMedicalRecordAccessService medicalRecordAccessService)
{
    public async Task ExecuteAsync(RegisterAllergyCommand command, CancellationToken cancellationToken = default)
    {
        var medicalRecord = await medicalRecordRepository
            .GetByIdAsync(new MedicalRecordId(command.MedicalRecordId), cancellationToken);

        if (medicalRecord is null)
            throw new ArgumentException($"MedicalRecord with id {command.MedicalRecordId} not found");

        // TODO: DoctorId should come from the authenticated user context, not be hardcoded
        var doctorId = new DoctorId(Guid.NewGuid());
        var canWrite = await medicalRecordAccessService.CanWriteAsync(
            doctorId: doctorId,
            patientId: medicalRecord.PatientId,
            cancellationToken);

        if (!canWrite)
            throw new ArgumentException($"MedicalRecord with id {command.MedicalRecordId} not found");

        var allergy = new Allergy(
            Substance: command.Substance,
            Severity: Enum.Parse<AllergySeverity>(command.Severity, ignoreCase: true));

        medicalRecord.RegisterAllergy(allergy, doctorId);

        await medicalRecordRepository.SaveAsync(medicalRecord, cancellationToken);
    }
}