using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;

public sealed class MedicalRecord
{
    private readonly List<IDomainEvent> _unpublishedEvents = [];
    private readonly List<Allergy> _allergies = [];

    public MedicalRecordId MedicalRecordId { get; private set; } = null!;
    public PatientId PatientId { get; private set; } = null!;
    public IReadOnlyList<Allergy> Allergies => _allergies.AsReadOnly();
    public IReadOnlyList<IDomainEvent> UnpublishedEvents => _unpublishedEvents.AsReadOnly();

    private MedicalRecord()
    {
    }

    public static MedicalRecord Create(MedicalRecordId id, PatientId patientId)
    {
        var medicalRecord = new MedicalRecord();
        medicalRecord.Raise(new MedicalRecordCreated(id, patientId, DateTime.UtcNow));
        return medicalRecord;
    }

    public void RegisterAllergy(Allergy allergy)
    {
        if (_allergies.Contains(allergy))
            throw new DomainException($"Allergy already registered for {allergy}");

        Raise(new AllergyRegistered(allergy, DateTime.UtcNow));
    }

    public bool HasAllergyTo(Medication medication)
    {
        return _allergies.Any(allergy => allergy.IsAllergicTo(medication));
    }

    public static MedicalRecord Reconstitute(IEnumerable<IDomainEvent> events)
    {
        var medicalRecord = new MedicalRecord();
        foreach (var @event in events)
        {
            medicalRecord.Apply(@event);
        }

        return medicalRecord;
    }

    private void Raise(IDomainEvent @event)
    {
        Apply(@event);
        _unpublishedEvents.Add(@event);
    }

    private void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case MedicalRecordCreated created:
                Apply(created);
                break;
            case AllergyRegistered allergyRegistered:
                Apply(allergyRegistered);
                break;
            default:
                throw new DomainException($"Event not supported: {@event.GetType().Name}");
        }
    }

    private void Apply(MedicalRecordCreated e)
    {
        MedicalRecordId = e.MedicalRecordId;
        PatientId = e.PatientId;
    }

    private void Apply(AllergyRegistered e)
    {
        _allergies.Add(e.Allergy);
    }
}