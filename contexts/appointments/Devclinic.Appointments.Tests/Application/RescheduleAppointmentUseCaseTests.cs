using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Application.Features.RescheduleAppointmentByDoctor;
using Devclinic.Appointments.Application.Features.RescheduleAppointmentByPatient;
using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.Enums;
using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

using NSubstitute;

namespace Devclinic.Appointments.Tests.Application;

public sealed class RescheduleAppointmentUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ByPatientWhenPatientOwnsAppointment_ShouldRescheduleUpdateAndPublishEvent()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var patientId = new PatientId(Guid.NewGuid());
        var appointment = CreateAppointment(patientId: patientId);
        var newTime = DateTime.UtcNow.AddDays(2);
        appointment.ClearDomainEvents();
        repository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        repository.GetByDoctorAndTimeAsync(appointment.DoctorId, Arg.Any<AppointmentTime>(),
                Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);
        repository.GetByPatientAndTimeAsync(appointment.PatientId, Arg.Any<AppointmentTime>(),
                Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);
        var useCase = new RescheduleAppointmentByPatientUseCase(repository, eventBus);

        await useCase.ExecuteAsync(new RescheduleAppointmentByPatientCommand(
            appointment.Id.Value,
            patientId.Value,
            newTime), TestContext.Current.CancellationToken);

        Assert.Equal(AppointmentStatus.Rescheduled, appointment.Status);
        Assert.Equal(newTime, appointment.Time.Value);
        await repository.Received(1).UpdateAsync(appointment, Arg.Any<CancellationToken>());
        await eventBus.Received(1).PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        Assert.Empty(appointment.DomainEvents);
    }

    [Fact]
    public async Task ExecuteAsync_ByDoctorWhenDoctorOwnsAppointment_ShouldRescheduleUpdateAndPublishEvent()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var doctorId = new DoctorId(Guid.NewGuid());
        var appointment = CreateAppointment(doctorId: doctorId);
        var newTime = DateTime.UtcNow.AddDays(2);
        appointment.ClearDomainEvents();
        repository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        repository.GetByDoctorAndTimeAsync(appointment.DoctorId, Arg.Any<AppointmentTime>(),
                Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);
        repository.GetByPatientAndTimeAsync(appointment.PatientId, Arg.Any<AppointmentTime>(),
                Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);
        var useCase = new RescheduleAppointmentByDoctorUseCase(repository, eventBus);

        await useCase.ExecuteAsync(new RescheduleAppointmentByDoctorCommand(
            appointment.Id.Value,
            doctorId.Value,
            newTime), TestContext.Current.CancellationToken);

        Assert.Equal(AppointmentStatus.Rescheduled, appointment.Status);
        Assert.Equal(newTime, appointment.Time.Value);
        await repository.Received(1).UpdateAsync(appointment, Arg.Any<CancellationToken>());
        await eventBus.Received(1).PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        Assert.Empty(appointment.DomainEvents);
    }

    [Fact]
    public async Task ExecuteAsync_ByPatientWhenTimeIsInPast_ShouldThrowInvalidOperationException()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var useCase = new RescheduleAppointmentByPatientUseCase(repository, eventBus);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(new RescheduleAppointmentByPatientCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-1)), TestContext.Current.CancellationToken));

        Assert.Equal("The new appointment time is in the past.", exception.Message);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ByPatientWhenDoctorHasConflict_ShouldThrowInvalidOperationException()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var patientId = new PatientId(Guid.NewGuid());
        var appointment = CreateAppointment(patientId: patientId);
        repository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        repository.GetByDoctorAndTimeAsync(appointment.DoctorId, Arg.Any<AppointmentTime>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateAppointment(doctorId: appointment.DoctorId));
        var useCase = new RescheduleAppointmentByPatientUseCase(repository, eventBus);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(new RescheduleAppointmentByPatientCommand(
                appointment.Id.Value,
                patientId.Value,
                DateTime.UtcNow.AddDays(2)), TestContext.Current.CancellationToken));

        Assert.Equal("The doctor already has an appointment scheduled for this time.", exception.Message);
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ByDoctorWhenPatientHasConflict_ShouldThrowInvalidOperationException()
    {
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var doctorId = new DoctorId(Guid.NewGuid());
        var appointment = CreateAppointment(doctorId: doctorId);
        repository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        repository.GetByDoctorAndTimeAsync(appointment.DoctorId, Arg.Any<AppointmentTime>(),
                Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);
        repository.GetByPatientAndTimeAsync(appointment.PatientId, Arg.Any<AppointmentTime>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateAppointment(patientId: appointment.PatientId));
        var useCase = new RescheduleAppointmentByDoctorUseCase(repository, eventBus);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(new RescheduleAppointmentByDoctorCommand(
                appointment.Id.Value,
                doctorId.Value,
                DateTime.UtcNow.AddDays(2)), TestContext.Current.CancellationToken));

        Assert.Equal("The patient already has an appointment scheduled for this time.", exception.Message);
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