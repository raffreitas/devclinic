namespace Devclinic.Appointments.Domain.ValueObjects;

public sealed record AppointmentTime(DateTime Value)
{
    public bool IsPast(DateTime referenceDate) => Value < referenceDate;
};