using Devclinic.MedicalRecords.Domain.Aggregates.Attendances;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Services;

public sealed class PrescriptionService
{
    public void IssuePrescription(
        Attendance attendance,
        MedicalRecord medicalRecord,
        DoctorId doctorId,
        Medication medication,
        Dosage dosage)
    {
        if (medicalRecord.HasAllergyTo(medication))
            throw new DomainException(
                $"Cannot prescribe '{medication.Name}' — patient has a registered allergy to '{medication.ActiveSubstance}'.");

        attendance.IssuePrescription(doctorId, medication, dosage);
    }
}