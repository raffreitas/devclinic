using Devclinic.Appointments.Common.SeedWork;

namespace Devclinic.Appointments.Domain.Events;

public sealed record AppointmentConfirmedEvent(
    PatientId PatientId,
    DoctorId DoctorId,
    AppointmentTime Time) : DomainEvent;