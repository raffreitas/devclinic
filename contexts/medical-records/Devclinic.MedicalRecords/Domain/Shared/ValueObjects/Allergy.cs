namespace Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

public sealed record Allergy(Substance Substance, AllergySeverity Severity)
{
    public bool IsAllergicTo(Medication medication)
        => Substance == medication.Substance;
};

public enum AllergySeverity
{
    Mild,
    Moderate,
    Severe
}