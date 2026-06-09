namespace Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

public sealed record Substance
{
    public string Value { get; }

    public Substance(string value)
    {
        Value = value.Trim().ToUpperInvariant();
    }

    public override string ToString() => Value;
}