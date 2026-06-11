using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace Devclinic.Appointments.Infrastructure.Data.Queries;

internal sealed class EfAppointmentQueries(AppointmentsDbContext dbContext) : IAppointmentQueries
{
    public async Task<AppointmentDetails?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var appointment = await dbContext.Appointments
            .AsNoTracking()
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(appointment => appointment.Id == new AppointmentId(id), ct);

        return appointment is null ? null : ToDetails(appointment);
    }

    public async Task<IReadOnlyList<AppointmentSummary>> GetByDoctorAsync(
        Guid doctorId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.ToDateTime(TimeOnly.MaxValue);

        var appointments = await dbContext.Appointments
            .AsNoTracking()
            .Where(appointment =>
                appointment.DoctorId.Value == doctorId &&
                appointment.Time.Value >= fromDateTime &&
                appointment.Time.Value <= toDateTime)
            .OrderBy(appointment => appointment.Time.Value)
            .Select(appointment => ToSummary(appointment))
            .ToArrayAsync(ct);

        return appointments;
    }

    private static AppointmentDetails ToDetails(Appointment appointment) =>
        new(
            appointment.Id.Value,
            appointment.PatientId.Value,
            appointment.DoctorId.Value,
            appointment.Time.Value,
            appointment.Status.ToString(),
            appointment.CancellationReason?.Value,
            appointment.StatusHistory
                .OrderBy(statusChange => statusChange.OccurredAt)
                .Select(statusChange => new StatusChangeSummary(
                    statusChange.Status.ToString(),
                    statusChange.OccurredAt))
                .ToArray());

    private static AppointmentSummary ToSummary(Appointment appointment) =>
        new(
            appointment.Id.Value,
            appointment.PatientId.Value,
            appointment.DoctorId.Value,
            appointment.Time.Value,
            appointment.Status.ToString(),
            appointment.CancellationReason?.Value);
}