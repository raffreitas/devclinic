using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Application.Features.ScheduleAppointment;

public sealed record ScheduleAppointmentCommand(
    Guid DoctorId,
    Guid PatientId,
    DateTime Time);

public sealed class ScheduleAppointmentUseCase(
    IDoctorService doctorService,
    IPatientService patientService,
    IAppointmentRepository appointmentRepository,
    IEventBus eventBus)
{
    public async Task<AppointmentId> ExecuteAsync(ScheduleAppointmentCommand command, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var time = new AppointmentTime(command.Time);
        if (time.IsPast(now))
            throw new InvalidOperationException("The appointment time is in the past.");

        var patientId = new PatientId(command.PatientId);
        if (!await patientService.ExistsAsync(patientId, ct))
            throw new InvalidOperationException("The patient does not exist.");


        var doctorId = new DoctorId(command.DoctorId);
        if (!await doctorService.ExistsAsync(doctorId, ct))
            throw new InvalidOperationException("The doctor does not exist.");

        var existingAppointment = await appointmentRepository.GetByDoctorAndTimeAsync(doctorId, time, ct);

        if (existingAppointment is not null)
            throw new InvalidOperationException("The doctor already has an appointment scheduled for this time.");

        var appointment = new Appointment(
            new AppointmentId(Guid.NewGuid()),
            patientId,
            doctorId,
            time
        );

        await appointmentRepository.AddAsync(appointment, ct);

        foreach (var @event in appointment.DomainEvents)
            await eventBus.PublishAsync(@event, ct);

        return appointment.Id;
    }
}