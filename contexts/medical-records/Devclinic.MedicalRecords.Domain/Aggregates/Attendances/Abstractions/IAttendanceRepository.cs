using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;

public interface IAttendanceRepository
{
    Task<Attendance?> GetByIdAsync(AttendanceId id, CancellationToken cancellationToken);

    Task SaveAsync(Attendance attendance, CancellationToken cancellationToken);
}
