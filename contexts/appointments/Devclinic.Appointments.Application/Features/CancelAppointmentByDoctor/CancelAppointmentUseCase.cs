using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Application.Features.CancelAppointmentByDoctor;

public sealed record CancelAppointmentByDoctorCommand(Guid DoctorId, Guid AppointmentId, string Reason);

public sealed class CancelAppointmentByDoctorUseCase(IAppointmentRepository repository, IEventBus eventBus)
{
    public async Task ExecuteAsync(CancelAppointmentByDoctorCommand command, CancellationToken ct = default)
    {
        var appointmentId = new AppointmentId(command.AppointmentId);
        var doctorId = new DoctorId(command.DoctorId);

        var appointment = await repository.GetByIdAsync(appointmentId, ct);
        if (appointment is null)
            throw new InvalidOperationException($"Appointment with id {command.AppointmentId} not found");

        if (appointment.DoctorId != doctorId)
            throw new InvalidOperationException("The doctor is not assigned to this appointment.");

        appointment.Cancel(command.Reason);

        await repository.UpdateAsync(appointment, ct);

        foreach (var @event in appointment.DomainEvents)
            await eventBus.PublishAsync(@event, ct);

        appointment.ClearDomainEvents();
    }
}