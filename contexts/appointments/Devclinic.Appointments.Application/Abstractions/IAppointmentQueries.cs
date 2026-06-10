namespace Devclinic.Appointments.Application.Abstractions;

public interface IAppointmentQueries
{
    Task<AppointmentDetails?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<IReadOnlyList<AppointmentSummary>> GetByDoctorAsync(
        Guid doctorId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);
}

public sealed record AppointmentDetails(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    DateTime Time,
    string Status,
    string? CancellationReason,
    IReadOnlyList<StatusChangeSummary> StatusChanges);

public sealed record AppointmentSummary(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    DateTime Time,
    string Status,
    string? CancellationReason);

public sealed record StatusChangeSummary(
    string Status,
    DateTime OccurredAt);