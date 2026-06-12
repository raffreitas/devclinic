namespace Devclinic.MedicalRecords.Infrastructure.EventSourcing.Projections;

public sealed class MedicalRecordIndex
{
    public Guid MedicalRecordId { get; init; }

    public Guid PatientId { get; init; }

    public string Status { get; set; } = string.Empty;
}