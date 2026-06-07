using Devclinic.Appointments.Common.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Domain.Events;

public sealed record AppointmentScheduled(
    AppointmentId AppointmentId,
    PatientId PatientId,
    DoctorId DoctorId,
    AppointmentTime Time) : DomainEvent;