using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Domain.Events;

public sealed record AppointmentConfirmedEvent(
    PatientId PatientId,
    DoctorId DoctorId,
    AppointmentTime Time) : DomainEvent;