using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.Enums;
using Devclinic.Appointments.Domain.Events;
using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.UnitTests.Domain;

public sealed class AppointmentTests
{
    [Fact]
    public void Constructor_ShouldCreateScheduledAppointmentAndRaiseEvent()
    {
        var id = new AppointmentId(Guid.NewGuid());
        var patientId = new PatientId(Guid.NewGuid());
        var doctorId = new DoctorId(Guid.NewGuid());
        var time = new AppointmentTime(DateTime.UtcNow.AddDays(1));

        var appointment = new Appointment(id, patientId, doctorId, time);

        Assert.Equal(id, appointment.Id);
        Assert.Equal(patientId, appointment.PatientId);
        Assert.Equal(doctorId, appointment.DoctorId);
        Assert.Equal(time, appointment.Time);
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.Single(appointment.StatusHistory);
        Assert.IsType<AppointmentScheduled>(Assert.Single(appointment.DomainEvents));
    }

    [Fact]
    public void Confirm_WhenScheduled_ShouldChangeStatusAndRaiseEvent()
    {
        var appointment = CreateAppointment();
        appointment.ClearDomainEvents();

        appointment.Confirm();

        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
        Assert.IsType<AppointmentConfirmed>(Assert.Single(appointment.DomainEvents));
    }

    [Fact]
    public void Confirm_WhenNotScheduled_ShouldThrowDomainException()
    {
        var appointment = CreateAppointment();
        appointment.Cancel("Patient requested cancellation");

        var exception = Assert.Throws<DomainException>(() => appointment.Confirm());

        Assert.Equal("Only scheduled appointments can be confirmed.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_WhenReasonIsBlank_ShouldThrowDomainException(string reason)
    {
        var appointment = CreateAppointment();

        var exception = Assert.Throws<DomainException>(() => appointment.Cancel(reason));

        Assert.Equal("A cancellation reason is required.", exception.Message);
    }

    [Fact]
    public void Cancel_WhenReasonIsValid_ShouldChangeStatusAndRaiseEvent()
    {
        var appointment = CreateAppointment();
        appointment.ClearDomainEvents();

        appointment.Cancel("Doctor unavailable");

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.Equal("Doctor unavailable", appointment.CancellationReason?.Value);
        Assert.IsType<AppointmentCancelled>(Assert.Single(appointment.DomainEvents));
    }

    [Fact]
    public void Reschedule_WhenAppointmentIsActive_ShouldChangeTimeStatusAndRaiseEvent()
    {
        var appointment = CreateAppointment();
        appointment.ClearDomainEvents();
        var newTime = new AppointmentTime(DateTime.UtcNow.AddDays(2));

        appointment.Reschedule(newTime);

        Assert.Equal(AppointmentStatus.Rescheduled, appointment.Status);
        Assert.Equal(newTime, appointment.Time);
        Assert.IsType<AppointmentRescheduled>(Assert.Single(appointment.DomainEvents));
    }

    [Fact]
    public void Reschedule_WhenCancelled_ShouldThrowDomainException()
    {
        var appointment = CreateAppointment();
        appointment.Cancel("Patient requested cancellation");

        var exception = Assert.Throws<DomainException>(
            () => appointment.Reschedule(new AppointmentTime(DateTime.UtcNow.AddDays(2))));

        Assert.Equal("Cancelled appointments cannot be rescheduled.", exception.Message);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemovePendingEvents()
    {
        var appointment = CreateAppointment();

        appointment.ClearDomainEvents();

        Assert.Empty(appointment.DomainEvents);
    }

    private static Appointment CreateAppointment()
    {
        return new Appointment(
            new AppointmentId(Guid.NewGuid()),
            new PatientId(Guid.NewGuid()),
            new DoctorId(Guid.NewGuid()),
            new AppointmentTime(DateTime.UtcNow.AddDays(1)));
    }
}
