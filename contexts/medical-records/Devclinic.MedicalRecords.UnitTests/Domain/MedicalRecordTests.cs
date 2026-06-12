using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.UnitTests.Domain;

public sealed class MedicalRecordTests
{
    [Fact]
    public void Create_ShouldCreateActiveMedicalRecordAndRaiseEvent()
    {
        var id = new MedicalRecordId(Guid.NewGuid());
        var patientId = new PatientId(Guid.NewGuid());

        var medicalRecord = MedicalRecord.Create(id, patientId);

        Assert.Equal(id, medicalRecord.Id);
        Assert.Equal(patientId, medicalRecord.PatientId);
        Assert.Equal(MedicalRecordStatus.Active, medicalRecord.Status);
        Assert.Empty(medicalRecord.Allergies);
        Assert.IsType<MedicalRecordCreated>(Assert.Single(medicalRecord.UnpublishedEvents));
    }

    [Fact]
    public void RegisterAllergy_WhenRecordIsActive_ShouldAddAllergyAndRaiseEvent()
    {
        var medicalRecord = CreateMedicalRecord();
        var doctorId = new DoctorId(Guid.NewGuid());
        var allergy = new Allergy("Dipyrone", AllergySeverity.Severe);
        medicalRecord.ClearUnpublishedEvents();

        medicalRecord.RegisterAllergy(allergy, doctorId);

        Assert.Contains(allergy, medicalRecord.Allergies);
        Assert.IsType<AllergyRegistered>(Assert.Single(medicalRecord.UnpublishedEvents));
    }

    [Fact]
    public void RegisterAllergy_WhenAllergyAlreadyExists_ShouldThrowDomainException()
    {
        var medicalRecord = CreateMedicalRecord();
        var doctorId = new DoctorId(Guid.NewGuid());
        var allergy = new Allergy("Dipyrone", AllergySeverity.Severe);
        medicalRecord.RegisterAllergy(allergy, doctorId);

        var exception = Assert.Throws<DomainException>(() => medicalRecord.RegisterAllergy(allergy, doctorId));

        Assert.Equal("Allergy to 'Dipyrone' is already registered.", exception.Message);
    }

    [Fact]
    public void RegisterAllergy_WhenRecordIsClosed_ShouldThrowDomainException()
    {
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.Close(MedicalRecordClosureReason.TransferRequested);

        var exception = Assert.Throws<DomainException>(() =>
            medicalRecord.RegisterAllergy(
                new Allergy("Dipyrone", AllergySeverity.Severe),
                new DoctorId(Guid.NewGuid())));

        Assert.Equal("Cannot modify a closed medical record.", exception.Message);
    }

    [Fact]
    public void Close_WhenRecordIsActive_ShouldCloseAndRaiseEvent()
    {
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.ClearUnpublishedEvents();

        medicalRecord.Close(MedicalRecordClosureReason.TransferRequested);

        Assert.Equal(MedicalRecordStatus.Closed, medicalRecord.Status);
        Assert.IsType<MedicalRecordClosed>(Assert.Single(medicalRecord.UnpublishedEvents));
    }

    [Fact]
    public void Close_WhenRecordIsAlreadyClosed_ShouldThrowDomainException()
    {
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.Close(MedicalRecordClosureReason.TransferRequested);

        var exception = Assert.Throws<DomainException>(() =>
            medicalRecord.Close(MedicalRecordClosureReason.TransferRequested));

        Assert.Equal("Medical record is already closed.", exception.Message);
    }

    [Fact]
    public void HasAllergyTo_WhenMedicationMatchesRegisteredAllergy_ShouldReturnTrue()
    {
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.RegisterAllergy(
            new Allergy("Dipyrone", AllergySeverity.Moderate),
            new DoctorId(Guid.NewGuid()));

        var result = medicalRecord.HasAllergyTo(new Medication("Novalgina", "dipyrone"));

        Assert.True(result);
    }

    [Fact]
    public void ClearUnpublishedEvents_ShouldRemovePendingEvents()
    {
        var medicalRecord = CreateMedicalRecord();

        medicalRecord.ClearUnpublishedEvents();

        Assert.Empty(medicalRecord.UnpublishedEvents);
    }

    private static MedicalRecord CreateMedicalRecord()
    {
        return MedicalRecord.Create(
            new MedicalRecordId(Guid.NewGuid()),
            new PatientId(Guid.NewGuid()));
    }
}