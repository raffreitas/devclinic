using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Services;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.UnitTests.Domain.Services;

public sealed class PrescriptionServiceTests
{
    [Fact]
    public void IssuePrescription_WhenMedicalRecordHasAllergy_ShouldThrowDomainException()
    {
        var service = new PrescriptionService();
        var attendance = CreateAttendance();
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.RegisterAllergy(
            new Allergy("Dipyrone", AllergySeverity.Severe),
            new DoctorId(Guid.NewGuid()));

        var exception = Assert.Throws<DomainException>(() =>
            service.IssuePrescription(
                attendance,
                medicalRecord,
                attendance.DoctorId,
                new Medication("Novalgina", "Dipyrone"),
                new Dosage("500mg", "8/8h", "3 days")));

        Assert.Equal(
            "Cannot prescribe 'Novalgina' — patient has a registered allergy to 'Dipyrone'.",
            exception.Message);
    }

    [Fact]
    public void IssuePrescription_WhenMedicalRecordHasNoAllergy_ShouldIssuePrescription()
    {
        var service = new PrescriptionService();
        var attendance = CreateAttendance();
        var medicalRecord = CreateMedicalRecord();
        attendance.ClearUnpublishedEvents();
        var medication = new Medication("Paracetamol", "Acetaminophen");
        var dosage = new Dosage("750mg", "8/8h", "5 days");

        service.IssuePrescription(attendance, medicalRecord, attendance.DoctorId, medication, dosage);

        var prescription = Assert.Single(attendance.Prescriptions);
        Assert.Equal(medication, prescription.Medication);
        Assert.Equal(dosage, prescription.Dosage);
        Assert.IsType<PrescriptionIssued>(Assert.Single(attendance.UnpublishedEvents));
    }

    private static Attendance CreateAttendance()
    {
        return Attendance.Start(
            new AttendanceId(Guid.NewGuid()),
            new MedicalRecordId(Guid.NewGuid()),
            new AppointmentId(Guid.NewGuid()),
            new DoctorId(Guid.NewGuid()));
    }

    private static MedicalRecord CreateMedicalRecord()
    {
        return MedicalRecord.Create(
            new MedicalRecordId(Guid.NewGuid()),
            new PatientId(Guid.NewGuid()));
    }
}