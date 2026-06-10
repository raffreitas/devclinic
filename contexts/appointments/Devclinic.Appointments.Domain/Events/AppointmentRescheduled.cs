using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Domain.Events;

public sealed record AppointmentRescheduled(
    AppointmentId AppointmentId,
    AppointmentTime NewTime,
    DateTime OccurredAt) : IDomainEvent;