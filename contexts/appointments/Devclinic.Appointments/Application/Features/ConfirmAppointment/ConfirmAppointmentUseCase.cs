using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Application.Features.ConfirmAppointment;

public sealed record ConfirmAppointmentCommand(Guid AppointmentId, Guid DoctorId);

public sealed class ConfirmAppointmentUseCase(IAppointmentRepository appointmentRepository, IEventBus eventBus)
{
    public async Task ExecuteAsync(ConfirmAppointmentCommand command, CancellationToken ct = default)
    {
        var appointmentId = new AppointmentId(command.AppointmentId);
        var doctorId = new DoctorId(command.DoctorId);

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, ct);

        if (appointment is null)
            throw new InvalidOperationException("The appointment does not exist.");

        if (appointment.DoctorId != doctorId)
            throw new InvalidOperationException("The doctor is not assigned to this appointment.");

        appointment.Confirm();

        await appointmentRepository.UpdateAsync(appointment, ct);

        foreach (var @event in appointment.DomainEvents)
            await eventBus.PublishAsync(@event, ct);

        appointment.ClearDomainEvents();
    }
}