using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.UnitTests.Domain.ValueObjects;

public sealed class AllergyTests
{
    [Fact]
    public void IsAllergicTo_WhenMedicationHasSameActiveSubstanceIgnoringCase_ShouldReturnTrue()
    {
        var allergy = new Allergy("Dipyrone", AllergySeverity.Severe);

        var result = allergy.IsAllergicTo(new Medication("Novalgina", "dipyrone"));

        Assert.True(result);
    }

    [Fact]
    public void IsAllergicTo_WhenMedicationHasDifferentActiveSubstance_ShouldReturnFalse()
    {
        var allergy = new Allergy("Dipyrone", AllergySeverity.Severe);

        var result = allergy.IsAllergicTo(new Medication("Paracetamol", "Acetaminophen"));

        Assert.False(result);
    }
}