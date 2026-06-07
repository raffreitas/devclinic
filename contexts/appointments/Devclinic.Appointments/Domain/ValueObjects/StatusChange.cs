using Devclinic.Appointments.Domain.Enums;

namespace Devclinic.Appointments.Domain.ValueObjects;

public sealed record StatusChange(
    AppointmentStatus Status,
    DateTime OccurredAt);