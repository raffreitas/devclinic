using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Application.Features.CancelAppointmentByPatient;

public sealed record CancelAppointmentByPatientCommand(Guid PatientId, Guid AppointmentId, string Reason);

public sealed class CancelAppointmentByPatientUseCase(IAppointmentRepository repository, IEventBus eventBus)
{
    public async Task ExecuteAsync(CancelAppointmentByPatientCommand command, CancellationToken ct = default)
    {
        var appointmentId = new AppointmentId(command.AppointmentId);
        var patientId = new PatientId(command.PatientId);

        var appointment = await repository.GetByIdAsync(appointmentId, ct);
        if (appointment is null)
            throw new InvalidOperationException($"Appointment with id {command.AppointmentId} not found");

        if (appointment.PatientId != patientId)
            throw new InvalidOperationException("The patient is not assigned to this appointment.");

        appointment.Cancel(command.Reason);

        await repository.UpdateAsync(appointment, ct);

        foreach (var @event in appointment.DomainEvents)
            await eventBus.PublishAsync(@event, ct);

        appointment.ClearDomainEvents();
    }
}