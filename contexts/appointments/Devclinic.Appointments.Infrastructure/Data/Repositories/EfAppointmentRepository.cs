using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace Devclinic.Appointments.Infrastructure.Data.Repositories;

internal sealed class EfAppointmentRepository(AppointmentsDbContext dbContext) : IAppointmentRepository
{
    public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
    {
        await dbContext.AddAsync(appointment, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken ct = default)
    {
        dbContext.Update(appointment);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<Appointment?> GetByIdAsync(AppointmentId appointmentId, CancellationToken ct = default)
    {
        return await dbContext.Appointments
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(appointment => appointment.Id == appointmentId, ct);
    }

    public async Task<Appointment?> GetByDoctorAndTimeAsync(DoctorId doctorId, AppointmentTime time,
        CancellationToken ct = default)
    {
        return await dbContext.Appointments.FirstOrDefaultAsync(appointment =>
            appointment.DoctorId == doctorId && appointment.Time == time, ct);
    }

    public Task<Appointment?> GetByPatientAndTimeAsync(PatientId patientId, AppointmentTime time,
        CancellationToken ct = default)
    {
        return dbContext.Appointments.FirstOrDefaultAsync(appointment =>
            appointment.PatientId == patientId && appointment.Time == time, ct);
    }
}