using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Services;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Features.IssuePrescription;

public sealed record IssuePrescriptionCommand(
    Guid AttendanceId,
    Guid DoctorId,
    string MedicationName,
    string ActiveSubstance,
    string Amount,
    string Frequency,
    string Duration);

public sealed class IssuePrescriptionUseCase(
    IAttendanceRepository attendanceRepository,
    IMedicalRecordRepository medicalRecordRepository,
    PrescriptionService prescriptionService)
{
    public async Task ExecuteAsync(IssuePrescriptionCommand command, CancellationToken ct = default)
    {
        var attendance = await attendanceRepository
            .GetByIdAsync(new AttendanceId(command.AttendanceId), ct);

        if (attendance is null)
            throw new ArgumentException($"Attendance with id {command.AttendanceId} not found");

        var medicalRecord = await medicalRecordRepository.GetByIdAsync(attendance.MedicalRecordId, ct);

        if (medicalRecord is null)
            throw new ArgumentException($"MedicalRecord with id {attendance.MedicalRecordId.Value} not found");

        prescriptionService.IssuePrescription(
            attendance,
            medicalRecord,
            new DoctorId(command.DoctorId),
            new Medication(command.MedicationName, command.ActiveSubstance),
            new Dosage(command.Amount, command.Frequency, command.Duration));

        await attendanceRepository.SaveAsync(attendance, ct);
    }
}
