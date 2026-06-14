using Devclinic.MedicalRecords.Application.Features.IssuePrescription;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Services;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

using NSubstitute;

namespace Devclinic.MedicalRecords.UnitTests.Application;

public sealed class IssuePrescriptionUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordHasAllergy_ShouldThrowDomainException()
    {
        var attendanceRepository = Substitute.For<IAttendanceRepository>();
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var attendance = CreateAttendance();
        var medicalRecord = CreateMedicalRecord(attendance.MedicalRecordId);
        medicalRecord.RegisterAllergy(new Allergy("Dipyrone", AllergySeverity.Severe), attendance.DoctorId);
        attendance.ClearUnpublishedEvents();
        medicalRecord.ClearUnpublishedEvents();
        attendanceRepository.GetByIdAsync(attendance.Id, Arg.Any<CancellationToken>()).Returns(attendance);
        medicalRecordRepository.GetByIdAsync(attendance.MedicalRecordId, Arg.Any<CancellationToken>())
            .Returns(medicalRecord);
        var useCase = new IssuePrescriptionUseCase(
            attendanceRepository,
            medicalRecordRepository,
            new PrescriptionService());

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            useCase.ExecuteAsync(
                new IssuePrescriptionCommand(
                    attendance.Id.Value,
                    attendance.DoctorId.Value,
                    "Novalgina",
                    "Dipyrone",
                    "500mg",
                    "8/8h",
                    "5 days"),
                TestContext.Current.CancellationToken));

        Assert.Contains("Cannot prescribe", exception.Message);
        await attendanceRepository.DidNotReceive().SaveAsync(Arg.Any<Attendance>(), Arg.Any<CancellationToken>());
    }

    private static MedicalRecord CreateMedicalRecord(MedicalRecordId medicalRecordId) =>
        MedicalRecord.Create(medicalRecordId, new PatientId(Guid.NewGuid()));

    private static Attendance CreateAttendance() =>
        Attendance.Start(
            new AttendanceId(Guid.NewGuid()),
            new MedicalRecordId(Guid.NewGuid()),
            new AppointmentId(Guid.NewGuid()),
            new DoctorId(Guid.NewGuid()));
}
