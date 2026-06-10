namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;

public sealed record Diagnosis(string CID, string Description, DiagnosisType Type);

public enum DiagnosisType
{
    Provisional,
    Definitive
}