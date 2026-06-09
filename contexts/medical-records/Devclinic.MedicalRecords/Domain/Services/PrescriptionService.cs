using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Services;

public class PrescriptionService
{
    public void IssuePrescription(Attendance attendance, MedicalRecord medicalRecord, Medication medication)
    {
        if (medicalRecord.HasAllergyTo(medication))
            throw new DomainException($"Patient is allergic to {medication.Substance}");
    }
}