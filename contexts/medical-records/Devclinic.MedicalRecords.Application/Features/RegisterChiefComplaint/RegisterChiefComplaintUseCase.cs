using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Features.RegisterChiefComplaint;

public sealed record RegisterChiefComplaintCommand(
    Guid AttendanceId,
    Guid DoctorId,
    string Description);

public sealed class RegisterChiefComplaintUseCase(IAttendanceRepository attendanceRepository)
{
    public async Task ExecuteAsync(RegisterChiefComplaintCommand command, CancellationToken ct = default)
    {
        var attendance = await attendanceRepository
            .GetByIdAsync(new AttendanceId(command.AttendanceId), ct);

        if (attendance is null)
            throw new ArgumentException($"Attendance with id {command.AttendanceId} not found");

        attendance.RegisterChiefComplaint(
            new DoctorId(command.DoctorId),
            new ChiefComplaint(command.Description));

        await attendanceRepository.SaveAsync(attendance, ct);
    }
}
