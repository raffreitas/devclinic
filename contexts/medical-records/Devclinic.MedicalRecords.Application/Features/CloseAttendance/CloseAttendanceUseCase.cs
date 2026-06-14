using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Features.CloseAttendance;

public sealed record CloseAttendanceCommand(
    Guid AttendanceId,
    Guid DoctorId);

public sealed class CloseAttendanceUseCase(IAttendanceRepository attendanceRepository)
{
    public async Task ExecuteAsync(CloseAttendanceCommand command, CancellationToken ct = default)
    {
        var attendance = await attendanceRepository
            .GetByIdAsync(new AttendanceId(command.AttendanceId), ct);

        if (attendance is null)
            throw new ArgumentException($"Attendance with id {command.AttendanceId} not found");

        attendance.Close(new DoctorId(command.DoctorId));

        await attendanceRepository.SaveAsync(attendance, ct);
    }
}
