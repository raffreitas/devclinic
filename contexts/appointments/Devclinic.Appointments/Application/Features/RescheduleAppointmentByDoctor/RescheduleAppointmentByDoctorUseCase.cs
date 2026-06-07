using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Application.Features.RescheduleAppointmentByDoctor;

public sealed record RescheduleAppointmentByDoctorCommand(
    Guid AppointmentId,
    Guid DoctorId,
    DateTime NewTime);

public sealed class RescheduleAppointmentByDoctorUseCase(
    IAppointmentRepository appointmentRepository,
    IEventBus eventBus)
{
    public async Task ExecuteAsync(RescheduleAppointmentByDoctorCommand command, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var newTime = new AppointmentTime(command.NewTime);
        if (newTime.IsPast(now))
            throw new InvalidOperationException("The new appointment time is in the past.");

        var appointment = await appointmentRepository.GetByIdAsync(new AppointmentId(command.AppointmentId), ct);
        if (appointment is null)
            throw new InvalidOperationException($"Appointment with id {command.AppointmentId} not found");

        if (appointment.DoctorId != new DoctorId(command.DoctorId))
            throw new InvalidOperationException("The doctor is not assigned to this appointment.");

        var doctorConflict = await appointmentRepository.GetByDoctorAndTimeAsync(appointment.DoctorId, newTime, ct);
        if (doctorConflict is not null && doctorConflict.Id != appointment.Id)
            throw new InvalidOperationException("The doctor already has an appointment scheduled for this time.");

        var patientConflict = await appointmentRepository.GetByPatientAndTimeAsync(appointment.PatientId, newTime, ct);
        if (patientConflict is not null && patientConflict.Id != appointment.Id)
            throw new InvalidOperationException("The patient already has an appointment scheduled for this time.");

        appointment.Reschedule(newTime);

        await appointmentRepository.UpdateAsync(appointment, ct);

        foreach (var @event in appointment.DomainEvents)
            await eventBus.PublishAsync(@event, ct);

        appointment.ClearDomainEvents();
    }
}