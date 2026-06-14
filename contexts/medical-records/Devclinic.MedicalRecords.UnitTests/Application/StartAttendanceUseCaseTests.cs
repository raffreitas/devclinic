using Devclinic.MedicalRecords.Application.Features.StartAttendance;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Devclinic.MedicalRecords.UnitTests.Application;

public sealed class StartAttendanceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordIsActive_ShouldStartAndSaveAttendance()
    {
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var attendanceRepository = Substitute.For<IAttendanceRepository>();
        var medicalRecord = CreateMedicalRecord();
        var command = new StartAttendanceCommand(
            medicalRecord.Id.Value,
            Guid.NewGuid(),
            Guid.NewGuid());
        medicalRecordRepository.GetByIdAsync(medicalRecord.Id, Arg.Any<CancellationToken>())
            .Returns(medicalRecord);
        var useCase = new StartAttendanceUseCase(medicalRecordRepository, attendanceRepository);

        var attendanceId = await useCase.ExecuteAsync(command, TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, attendanceId.Value);
        await attendanceRepository.Received(1).SaveAsync(
            Arg.Is<Attendance>(attendance =>
                attendance.Id == attendanceId &&
                attendance.MedicalRecordId == medicalRecord.Id &&
                attendance.AppointmentId == new AppointmentId(command.AppointmentId) &&
                attendance.DoctorId == new DoctorId(command.DoctorId) &&
                attendance.Status == AttendanceStatus.InProgress),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordDoesNotExist_ShouldThrowArgumentException()
    {
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var attendanceRepository = Substitute.For<IAttendanceRepository>();
        var medicalRecordId = Guid.NewGuid();
        medicalRecordRepository.GetByIdAsync(new MedicalRecordId(medicalRecordId), Arg.Any<CancellationToken>())
            .ReturnsNull();
        var useCase = new StartAttendanceUseCase(medicalRecordRepository, attendanceRepository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(
                new StartAttendanceCommand(medicalRecordId, Guid.NewGuid(), Guid.NewGuid()),
                TestContext.Current.CancellationToken));

        Assert.Equal($"MedicalRecord with id {medicalRecordId} not found", exception.Message);
        await attendanceRepository.DidNotReceive().SaveAsync(Arg.Any<Attendance>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordIsClosed_ShouldThrowInvalidOperationException()
    {
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var attendanceRepository = Substitute.For<IAttendanceRepository>();
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.Close(MedicalRecordClosureReason.TransferRequested);
        medicalRecordRepository.GetByIdAsync(medicalRecord.Id, Arg.Any<CancellationToken>())
            .Returns(medicalRecord);
        var useCase = new StartAttendanceUseCase(medicalRecordRepository, attendanceRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                new StartAttendanceCommand(medicalRecord.Id.Value, Guid.NewGuid(), Guid.NewGuid()),
                TestContext.Current.CancellationToken));

        Assert.Equal("Cannot start attendance for a closed medical record.", exception.Message);
        await attendanceRepository.DidNotReceive().SaveAsync(Arg.Any<Attendance>(), Arg.Any<CancellationToken>());
    }

    private static MedicalRecord CreateMedicalRecord() =>
        MedicalRecord.Create(
            new MedicalRecordId(Guid.NewGuid()),
            new PatientId(Guid.NewGuid()));
}
