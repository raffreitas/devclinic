using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Features.StartAttendance;

public sealed record StartAttendanceCommand(
    Guid MedicalRecordId,
    Guid AppointmentId,
    Guid DoctorId);

public sealed class StartAttendanceUseCase(
    IMedicalRecordRepository medicalRecordRepository,
    IAttendanceRepository attendanceRepository)
{
    public async Task<AttendanceId> ExecuteAsync(StartAttendanceCommand command, CancellationToken ct = default)
    {
        var medicalRecord = await medicalRecordRepository
            .GetByIdAsync(new MedicalRecordId(command.MedicalRecordId), ct);

        if (medicalRecord is null)
            throw new ArgumentException($"MedicalRecord with id {command.MedicalRecordId} not found");

        if (medicalRecord.Status == MedicalRecordStatus.Closed)
            throw new InvalidOperationException("Cannot start attendance for a closed medical record.");

        var attendanceId = new AttendanceId(Guid.CreateVersion7());
        var attendance = Attendance.Start(
            attendanceId,
            medicalRecord.Id,
            new AppointmentId(command.AppointmentId),
            new DoctorId(command.DoctorId));

        await attendanceRepository.SaveAsync(attendance, ct);

        return attendance.Id;
    }
}
