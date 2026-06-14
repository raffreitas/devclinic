using Devclinic.MedicalRecords.Application.Features.CloseAttendance;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

using NSubstitute;

namespace Devclinic.MedicalRecords.UnitTests.Application;

public sealed class CloseAttendanceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenDiagnosisExists_ShouldCloseAndSave()
    {
        var attendanceRepository = Substitute.For<IAttendanceRepository>();
        var attendance = CreateAttendance();
        attendance.RegisterDiagnosis(attendance.DoctorId, new Diagnosis("A00", "Cholera", DiagnosisType.Definitive));
        attendance.ClearUnpublishedEvents();
        attendanceRepository.GetByIdAsync(attendance.Id, Arg.Any<CancellationToken>()).Returns(attendance);
        var useCase = new CloseAttendanceUseCase(attendanceRepository);

        await useCase.ExecuteAsync(
            new CloseAttendanceCommand(attendance.Id.Value, attendance.DoctorId.Value),
            TestContext.Current.CancellationToken);

        Assert.Equal(AttendanceStatus.Closed, attendance.Status);
        await attendanceRepository.Received(1).SaveAsync(attendance, Arg.Any<CancellationToken>());
    }

    private static Attendance CreateAttendance() =>
        Attendance.Start(
            new AttendanceId(Guid.NewGuid()),
            new MedicalRecordId(Guid.NewGuid()),
            new AppointmentId(Guid.NewGuid()),
            new DoctorId(Guid.NewGuid()));
}
