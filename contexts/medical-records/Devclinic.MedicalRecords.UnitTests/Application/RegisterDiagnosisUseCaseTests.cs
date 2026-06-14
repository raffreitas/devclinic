using Devclinic.MedicalRecords.Application.Features.RegisterDiagnosis;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Devclinic.MedicalRecords.UnitTests.Application;

public sealed class RegisterDiagnosisUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenAttendanceDoesNotExist_ShouldThrowArgumentException()
    {
        var attendanceRepository = Substitute.For<IAttendanceRepository>();
        var attendanceId = Guid.NewGuid();
        attendanceRepository.GetByIdAsync(new AttendanceId(attendanceId), Arg.Any<CancellationToken>())
            .ReturnsNull();
        var useCase = new RegisterDiagnosisUseCase(attendanceRepository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(
                new RegisterDiagnosisCommand(attendanceId, Guid.NewGuid(), "A00", "Cholera", "Definitive"),
                TestContext.Current.CancellationToken));

        Assert.Equal($"Attendance with id {attendanceId} not found", exception.Message);
        await attendanceRepository.DidNotReceive().SaveAsync(Arg.Any<Attendance>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAttendanceExists_ShouldRegisterAndSave()
    {
        var attendanceRepository = Substitute.For<IAttendanceRepository>();
        var attendance = CreateAttendance();
        attendance.ClearUnpublishedEvents();
        attendanceRepository.GetByIdAsync(attendance.Id, Arg.Any<CancellationToken>()).Returns(attendance);
        var useCase = new RegisterDiagnosisUseCase(attendanceRepository);

        await useCase.ExecuteAsync(
            new RegisterDiagnosisCommand(attendance.Id.Value, attendance.DoctorId.Value, "A00", "Cholera", "Definitive"),
            TestContext.Current.CancellationToken);

        var diagnosis = Assert.Single(attendance.Diagnoses);
        Assert.Equal("A00", diagnosis.CID);
        Assert.Equal(DiagnosisType.Definitive, diagnosis.Type);
        await attendanceRepository.Received(1).SaveAsync(attendance, Arg.Any<CancellationToken>());
    }

    private static Attendance CreateAttendance() =>
        Attendance.Start(
            new AttendanceId(Guid.NewGuid()),
            new MedicalRecordId(Guid.NewGuid()),
            new AppointmentId(Guid.NewGuid()),
            new DoctorId(Guid.NewGuid()));
}
