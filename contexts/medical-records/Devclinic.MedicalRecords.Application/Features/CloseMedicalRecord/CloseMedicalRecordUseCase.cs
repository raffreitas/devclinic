using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Features.CloseMedicalRecord;

public sealed record CloseMedicalRecordCommand(
    Guid MedicalRecordId,
    string Reason);

internal sealed class CloseMedicalRecordUseCase(IMedicalRecordRepository medicalRecordRepository)
{
    public async Task ExecuteAsync(CloseMedicalRecordCommand command, CancellationToken ct = default)
    {
        var medicalRecord = await medicalRecordRepository
            .GetByIdAsync(new MedicalRecordId(command.MedicalRecordId), ct);

        if (medicalRecord is null)
            throw new ArgumentException($"MedicalRecord with id {command.MedicalRecordId} not found");

        var closeReason = Enum.Parse<MedicalRecordClosureReason>(command.Reason, ignoreCase: true);
        medicalRecord.Close(closeReason);

        await medicalRecordRepository.SaveAsync(medicalRecord, ct);
    }
}