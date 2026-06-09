using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Entities;

public sealed class Prescription
{
    public PrescriptionId Id { get; private set; }
    public Medication Medication { get; private set; }
    public Dosage Dosage { get; private set; }
    public PrescriptionStatus Status { get; private set; }

    public Prescription(PrescriptionId id, Medication medication, Dosage dosage)
    {
        Id = id;
        Medication = medication;
        Dosage = dosage;
        Status = PrescriptionStatus.Active;
    }

    public void Revoke()
    {
        if (Status == PrescriptionStatus.Revoked)
            throw new DomainException("Prescription is already revoked.");

        Status = PrescriptionStatus.Revoked;
    }
}

public enum PrescriptionStatus
{
    Active,
    Revoked
}