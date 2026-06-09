namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;

public sealed record Dosage(string Amount, string Frequency, string Duration);