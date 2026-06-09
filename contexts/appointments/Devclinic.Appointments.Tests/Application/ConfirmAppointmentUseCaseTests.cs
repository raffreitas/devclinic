using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Application.Features.ConfirmAppointment;
using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.Enums;
using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Devclinic.Appointments.Tests.Application;

public sealed class ConfirmAppointmentUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenDoctorOwnsAppointment_ShouldConfirmUpdateAndPublishEvent()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var doctorId = new DoctorId(Guid.NewGuid());
        var appointment = CreateAppointment(doctorId: doctorId);
        appointment.ClearDomainEvents();
        repository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        var useCase = new ConfirmAppointmentUseCase(repository, eventBus);

        await useCase.ExecuteAsync(
            new ConfirmAppointmentCommand(appointment.Id.Value, doctorId.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
        await repository.Received(1).UpdateAsync(appointment, Arg.Any<CancellationToken>());
        await eventBus.Received(1).PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        Assert.Empty(appointment.DomainEvents);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAppointmentDoesNotExist_ShouldThrowInvalidOperationException()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var appointmentId = Guid.NewGuid();
        repository.GetByIdAsync(new AppointmentId(appointmentId), Arg.Any<CancellationToken>())
            .ReturnsNull();
        var useCase = new ConfirmAppointmentUseCase(repository, eventBus);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(
            new ConfirmAppointmentCommand(appointmentId, Guid.NewGuid()),
            TestContext.Current.CancellationToken));

        Assert.Equal("The appointment does not exist.", exception.Message);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoctorDoesNotOwnAppointment_ShouldThrowInvalidOperationException()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var appointment = CreateAppointment();
        repository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        var useCase = new ConfirmAppointmentUseCase(repository, eventBus);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(
            new ConfirmAppointmentCommand(appointment.Id.Value, Guid.NewGuid()),
            TestContext.Current.CancellationToken));

        Assert.Equal("The doctor is not assigned to this appointment.", exception.Message);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    private static Appointment CreateAppointment(DoctorId? doctorId = null)
    {
        return new Appointment(
            new AppointmentId(Guid.NewGuid()),
            new PatientId(Guid.NewGuid()),
            doctorId ?? new DoctorId(Guid.NewGuid()),
            new AppointmentTime(DateTime.UtcNow.AddDays(1)));
    }
}