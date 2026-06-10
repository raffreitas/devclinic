using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Application.Features.CancelAppointmentByDoctor;
using Devclinic.Appointments.Application.Features.CancelAppointmentByPatient;
using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.Enums;
using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Devclinic.Appointments.UnitTests.Application;

public sealed class CancelAppointmentUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ByPatientWhenPatientOwnsAppointment_ShouldCancelUpdateAndPublishEvent()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var patientId = new PatientId(Guid.NewGuid());
        var appointment = CreateAppointment(patientId: patientId);
        appointment.ClearDomainEvents();
        repository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        var useCase = new CancelAppointmentByPatientUseCase(repository, eventBus);

        await useCase.ExecuteAsync(new CancelAppointmentByPatientCommand(
            patientId.Value,
            appointment.Id.Value,
            "Patient unavailable"), TestContext.Current.CancellationToken);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        await repository.Received(1).UpdateAsync(appointment, Arg.Any<CancellationToken>());
        await eventBus.Received(1).PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        Assert.Empty(appointment.DomainEvents);
    }

    [Fact]
    public async Task ExecuteAsync_ByDoctorWhenDoctorOwnsAppointment_ShouldCancelUpdateAndPublishEvent()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var doctorId = new DoctorId(Guid.NewGuid());
        var appointment = CreateAppointment(doctorId: doctorId);
        appointment.ClearDomainEvents();
        repository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        var useCase = new CancelAppointmentByDoctorUseCase(repository, eventBus);

        await useCase.ExecuteAsync(new CancelAppointmentByDoctorCommand(
            doctorId.Value,
            appointment.Id.Value,
            "Doctor unavailable"), TestContext.Current.CancellationToken);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        await repository.Received(1).UpdateAsync(appointment, Arg.Any<CancellationToken>());
        await eventBus.Received(1).PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        Assert.Empty(appointment.DomainEvents);
    }

    [Fact]
    public async Task ExecuteAsync_ByPatientWhenAppointmentDoesNotExist_ShouldThrowInvalidOperationException()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var appointmentId = Guid.NewGuid();
        repository.GetByIdAsync(new AppointmentId(appointmentId), Arg.Any<CancellationToken>())
            .ReturnsNull();
        var useCase = new CancelAppointmentByPatientUseCase(repository, eventBus);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(
            new CancelAppointmentByPatientCommand(Guid.NewGuid(), appointmentId, "Reason"),
            TestContext.Current.CancellationToken));

        Assert.Equal($"Appointment with id {appointmentId} not found", exception.Message);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ByDoctorWhenDoctorDoesNotOwnAppointment_ShouldThrowInvalidOperationException()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var appointment = CreateAppointment();
        repository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        var useCase = new CancelAppointmentByDoctorUseCase(repository, eventBus);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(
            new CancelAppointmentByDoctorCommand(Guid.NewGuid(), appointment.Id.Value, "Reason"),
            TestContext.Current.CancellationToken));

        Assert.Equal("The doctor is not assigned to this appointment.", exception.Message);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    private static Appointment CreateAppointment(PatientId? patientId = null, DoctorId? doctorId = null)
    {
        return new Appointment(
            new AppointmentId(Guid.NewGuid()),
            patientId ?? new PatientId(Guid.NewGuid()),
            doctorId ?? new DoctorId(Guid.NewGuid()),
            new AppointmentTime(DateTime.UtcNow.AddDays(1)));
    }
}