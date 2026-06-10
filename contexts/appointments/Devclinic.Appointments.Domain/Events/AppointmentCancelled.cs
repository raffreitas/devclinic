using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Domain.Events;

public sealed record AppointmentCancelled(
    AppointmentId AppointmentId,
    CancellationReason Reason,
    DateTime OccurredAt) : IDomainEvent;