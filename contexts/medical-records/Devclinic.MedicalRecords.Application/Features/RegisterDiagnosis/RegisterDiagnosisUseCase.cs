using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Application.Features.RegisterDiagnosis;

public sealed record RegisterDiagnosisCommand(
    Guid AttendanceId,
    Guid DoctorId,
    string Cid,
    string Description,
    string Type);

public sealed class RegisterDiagnosisUseCase(IAttendanceRepository attendanceRepository)
{
    public async Task ExecuteAsync(RegisterDiagnosisCommand command, CancellationToken ct = default)
    {
        var attendance = await attendanceRepository
            .GetByIdAsync(new AttendanceId(command.AttendanceId), ct);

        if (attendance is null)
            throw new ArgumentException($"Attendance with id {command.AttendanceId} not found");

        var diagnosis = new Diagnosis(
            command.Cid,
            command.Description,
            Enum.Parse<DiagnosisType>(command.Type, ignoreCase: true));

        attendance.RegisterDiagnosis(new DoctorId(command.DoctorId), diagnosis);

        await attendanceRepository.SaveAsync(attendance, ct);
    }
}
