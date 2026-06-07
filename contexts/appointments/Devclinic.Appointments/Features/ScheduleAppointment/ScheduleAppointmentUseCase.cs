using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Features.ScheduleAppointment;

public sealed record ScheduleAppointmentCommand(
    Guid DoctorId,
    Guid PatientId,
    DateTime Time);

public sealed class ScheduleAppointmentUseCase(IAppointmentRepository appointmentRepository)
{
    public async Task<AppointmentId> ExecuteAsync(ScheduleAppointmentCommand command, CancellationToken ct = default)
    {
        // Ideally, we should first verify whether the doctor and patient
        // exist, but this part was simplified.
        var time = new AppointmentTime(command.Time);
        var doctorId = new DoctorId(command.DoctorId);

        var existingAppointment = await appointmentRepository.GetByDoctorAndTimeAsync(doctorId, time, ct);

        if (existingAppointment is not null)
            throw new InvalidOperationException("The doctor already has an appointment scheduled for this time.");

        var appointment = new Appointment(
            new AppointmentId(Guid.NewGuid()),
            new PatientId(command.PatientId),
            doctorId,
            time
        );

        await appointmentRepository.AddAsync(appointment, ct);

        return appointment.Id;
    }
}