using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Domain.Events;

public sealed record AppointmentConfirmed(
    AppointmentId AppointmentId,
    AppointmentTime Time,
    DateTime OccurredAt) : IDomainEvent;