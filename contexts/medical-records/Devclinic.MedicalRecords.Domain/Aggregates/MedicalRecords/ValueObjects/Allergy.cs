using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;

public sealed record Allergy(string Substance, AllergySeverity Severity)
{
    public bool IsAllergicTo(Medication medication) =>
        medication.ActiveSubstance.Equals(Substance, StringComparison.OrdinalIgnoreCase);
}

public enum AllergySeverity
{
    Mild,
    Moderate,
    Severe
}