using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Domain.Events;

public sealed record AppointmentCancelledEvent(
    AppointmentId AppointmentId,
    PatientId PatientId,
    DoctorId DoctorId,
    CancellationReason Reason
) : DomainEvent;