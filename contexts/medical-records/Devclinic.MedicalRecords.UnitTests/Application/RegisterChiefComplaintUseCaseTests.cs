using Devclinic.MedicalRecords.Application.Features.RegisterChiefComplaint;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

using NSubstitute;

namespace Devclinic.MedicalRecords.UnitTests.Application;

public sealed class RegisterChiefComplaintUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenAttendanceExists_ShouldRegisterAndSave()
    {
        var attendanceRepository = Substitute.For<IAttendanceRepository>();
        var attendance = CreateAttendance();
        attendance.ClearUnpublishedEvents();
        attendanceRepository.GetByIdAsync(attendance.Id, Arg.Any<CancellationToken>()).Returns(attendance);
        var useCase = new RegisterChiefComplaintUseCase(attendanceRepository);

        await useCase.ExecuteAsync(
            new RegisterChiefComplaintCommand(attendance.Id.Value, attendance.DoctorId.Value, "Headache"),
            TestContext.Current.CancellationToken);

        Assert.Equal("Headache", attendance.ChiefComplaint?.Description);
        await attendanceRepository.Received(1).SaveAsync(attendance, Arg.Any<CancellationToken>());
    }

    private static Attendance CreateAttendance() =>
        Attendance.Start(
            new AttendanceId(Guid.NewGuid()),
            new MedicalRecordId(Guid.NewGuid()),
            new AppointmentId(Guid.NewGuid()),
            new DoctorId(Guid.NewGuid()));
}
