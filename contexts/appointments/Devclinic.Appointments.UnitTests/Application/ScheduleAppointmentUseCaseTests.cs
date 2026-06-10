using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Application.Features.ScheduleAppointment;
using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Devclinic.Appointments.UnitTests.Application;

public sealed class ScheduleAppointmentUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCommandIsValid_ShouldCreateAppointmentAndPublishEvent()
    {
        var doctorService = Substitute.For<IDoctorService>();
        var patientService = Substitute.For<IPatientService>();
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        patientService.ExistsAsync(Arg.Any<PatientId>(), Arg.Any<CancellationToken>()).Returns(true);
        doctorService.ExistsAsync(Arg.Any<DoctorId>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.GetByDoctorAndTimeAsync(Arg.Any<DoctorId>(), Arg.Any<AppointmentTime>(),
                Arg.Any<CancellationToken>())
            .ReturnsNull();

        var useCase = new ScheduleAppointmentUseCase(doctorService, patientService, repository, eventBus);

        var appointmentId = await useCase.ExecuteAsync(command, TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, appointmentId.Value);
        await repository.Received(1).AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
        await eventBus.Received(1).PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTimeIsInPast_ShouldThrowInvalidOperationException()
    {
        var useCase = CreateUseCase();
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(-1));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(command, TestContext.Current.CancellationToken));

        Assert.Equal("The appointment time is in the past.", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPatientDoesNotExist_ShouldThrowInvalidOperationException()
    {
        var doctorService = Substitute.For<IDoctorService>();
        var patientService = Substitute.For<IPatientService>();
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        patientService.ExistsAsync(Arg.Any<PatientId>(), Arg.Any<CancellationToken>()).Returns(false);
        var useCase = new ScheduleAppointmentUseCase(doctorService, patientService, repository, eventBus);
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(command, TestContext.Current.CancellationToken));

        Assert.Equal("The patient does not exist.", exception.Message);
        await repository.DidNotReceive().AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoctorDoesNotExist_ShouldThrowInvalidOperationException()
    {
        var doctorService = Substitute.For<IDoctorService>();
        var patientService = Substitute.For<IPatientService>();
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        patientService.ExistsAsync(Arg.Any<PatientId>(), Arg.Any<CancellationToken>()).Returns(true);
        doctorService.ExistsAsync(Arg.Any<DoctorId>(), Arg.Any<CancellationToken>()).Returns(false);
        var useCase = new ScheduleAppointmentUseCase(doctorService, patientService, repository, eventBus);
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(command, TestContext.Current.CancellationToken));

        Assert.Equal("The doctor does not exist.", exception.Message);
        await repository.DidNotReceive().AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoctorHasAppointmentAtTime_ShouldThrowInvalidOperationException()
    {
        var doctorService = Substitute.For<IDoctorService>();
        var patientService = Substitute.For<IPatientService>();
        var repository = Substitute.For<IAppointmentRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var existingAppointment = CreateAppointment();
        patientService.ExistsAsync(Arg.Any<PatientId>(), Arg.Any<CancellationToken>()).Returns(true);
        doctorService.ExistsAsync(Arg.Any<DoctorId>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.GetByDoctorAndTimeAsync(Arg.Any<DoctorId>(), Arg.Any<AppointmentTime>(),
                Arg.Any<CancellationToken>())
            .Returns(existingAppointment);
        var useCase = new ScheduleAppointmentUseCase(doctorService, patientService, repository, eventBus);
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(command, TestContext.Current.CancellationToken));

        Assert.Equal("The doctor already has an appointment scheduled for this time.", exception.Message);
        await repository.DidNotReceive().AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    private static ScheduleAppointmentUseCase CreateUseCase()
    {
        return new ScheduleAppointmentUseCase(
            Substitute.For<IDoctorService>(),
            Substitute.For<IPatientService>(),
            Substitute.For<IAppointmentRepository>(),
            Substitute.For<IEventBus>());
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