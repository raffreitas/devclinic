using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.UnitTests.Domain;

public sealed class AttendanceTests
{
    [Fact]
    public void Start_ShouldCreateInProgressAttendanceAndRaiseEvent()
    {
        var id = new AttendanceId(Guid.NewGuid());
        var medicalRecordId = new MedicalRecordId(Guid.NewGuid());
        var appointmentId = new AppointmentId(Guid.NewGuid());
        var doctorId = new DoctorId(Guid.NewGuid());

        var attendance = Attendance.Start(id, medicalRecordId, appointmentId, doctorId);

        Assert.Equal(id, attendance.Id);
        Assert.Equal(medicalRecordId, attendance.MedicalRecordId);
        Assert.Equal(appointmentId, attendance.AppointmentId);
        Assert.Equal(doctorId, attendance.DoctorId);
        Assert.Equal(AttendanceStatus.InProgress, attendance.Status);
        Assert.IsType<AttendanceStarted>(Assert.Single(attendance.UnpublishedEvents));
    }

    [Fact]
    public void RegisterDiagnosis_WhenInProgressAndDoctorIsResponsible_ShouldAddDiagnosisAndRaiseEvent()
    {
        var attendance = CreateAttendance();
        var diagnosis = new Diagnosis("A00", "Cholera", DiagnosisType.Definitive);
        attendance.ClearUnpublishedEvents();

        attendance.RegisterDiagnosis(attendance.DoctorId, diagnosis);

        Assert.Contains(diagnosis, attendance.Diagnoses);
        Assert.IsType<DiagnosisRegistered>(Assert.Single(attendance.UnpublishedEvents));
    }

    [Fact]
    public void RegisterDiagnosis_WhenDoctorIsNotResponsible_ShouldThrowDomainException()
    {
        var attendance = CreateAttendance();

        var exception = Assert.Throws<DomainException>(() =>
            attendance.RegisterDiagnosis(
                new DoctorId(Guid.NewGuid()),
                new Diagnosis("A00", "Cholera", DiagnosisType.Definitive)));

        Assert.Equal("Only the responsible doctor can modify this attendance.", exception.Message);
    }

    [Fact]
    public void RegisterChiefComplaint_WhenAttendanceIsClosed_ShouldThrowDomainException()
    {
        var attendance = CreateAttendance();
        attendance.RegisterDiagnosis(
            attendance.DoctorId,
            new Diagnosis("A00", "Cholera", DiagnosisType.Definitive));
        attendance.Close(attendance.DoctorId);

        var exception = Assert.Throws<DomainException>(() =>
            attendance.RegisterChiefComplaint(
                attendance.DoctorId,
                new ChiefComplaint("Headache")));

        Assert.Equal("Cannot modify a closed attendance.", exception.Message);
    }

    [Fact]
    public void IssuePrescription_WhenInProgressAndDoctorIsResponsible_ShouldAddPrescriptionAndRaiseEvent()
    {
        var attendance = CreateAttendance();
        var medication = new Medication("Novalgina", "Dipyrone");
        var dosage = new Dosage("500mg", "8/8h", "5 days");
        attendance.ClearUnpublishedEvents();

        attendance.IssuePrescription(attendance.DoctorId, medication, dosage);

        var prescription = Assert.Single(attendance.Prescriptions);
        Assert.Equal(medication, prescription.Medication);
        Assert.Equal(dosage, prescription.Dosage);
        Assert.IsType<PrescriptionIssued>(Assert.Single(attendance.UnpublishedEvents));
    }

    [Fact]
    public void Close_WhenNoDiagnosisIsRegistered_ShouldThrowDomainException()
    {
        var attendance = CreateAttendance();

        var exception = Assert.Throws<DomainException>(() => attendance.Close(attendance.DoctorId));

        Assert.Equal("At least one diagnosis is required before closing the attendance.", exception.Message);
    }

    [Fact]
    public void Close_WhenDiagnosisExists_ShouldCloseAndRaiseEvent()
    {
        var attendance = CreateAttendance();
        attendance.RegisterDiagnosis(
            attendance.DoctorId,
            new Diagnosis("A00", "Cholera", DiagnosisType.Definitive));
        attendance.ClearUnpublishedEvents();

        attendance.Close(attendance.DoctorId);

        Assert.Equal(AttendanceStatus.Closed, attendance.Status);
        Assert.Equal(attendance.DoctorId, attendance.ClosedBy);
        Assert.IsType<AttendanceClosed>(Assert.Single(attendance.UnpublishedEvents));
    }

    [Fact]
    public void ClearUnpublishedEvents_ShouldRemovePendingEvents()
    {
        var attendance = CreateAttendance();

        attendance.ClearUnpublishedEvents();

        Assert.Empty(attendance.UnpublishedEvents);
    }

    private static Attendance CreateAttendance()
    {
        return Attendance.Start(
            new AttendanceId(Guid.NewGuid()),
            new MedicalRecordId(Guid.NewGuid()),
            new AppointmentId(Guid.NewGuid()),
            new DoctorId(Guid.NewGuid()));
    }
}