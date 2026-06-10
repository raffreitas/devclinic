using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.UnitTests.Domain.ValueObjects;

public sealed class AppointmentTimeTests
{
    [Fact]
    public void IsPast_WhenValueIsBeforeReferenceDate_ShouldReturnTrue()
    {
        var referenceDate = DateTime.UtcNow;
        var appointmentTime = new AppointmentTime(referenceDate.AddTicks(-1));

        var result = appointmentTime.IsPast(referenceDate);

        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void IsPast_WhenValueIsEqualOrAfterReferenceDate_ShouldReturnFalse(int secondsToAdd)
    {
        var referenceDate = DateTime.UtcNow;
        var appointmentTime = new AppointmentTime(referenceDate.AddSeconds(secondsToAdd));

        var result = appointmentTime.IsPast(referenceDate);

        Assert.False(result);
    }
}
