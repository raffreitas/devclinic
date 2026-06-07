using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Domain.Abstractions;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment, CancellationToken ct = default);
    Task UpdateAsync(Appointment appointment, CancellationToken ct = default);

    Task<Appointment?> GetByIdAsync(AppointmentId appointmentId, CancellationToken ct = default);
    Task<Appointment?> GetByDoctorAndTimeAsync(DoctorId doctorId, AppointmentTime time, CancellationToken ct = default);
}