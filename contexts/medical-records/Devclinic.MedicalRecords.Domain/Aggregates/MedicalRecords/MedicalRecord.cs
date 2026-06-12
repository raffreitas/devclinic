using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;

public sealed class MedicalRecord
{
    private readonly List<IDomainEvent> _unpublishedEvents = [];
    private readonly List<Allergy> _allergies = [];

    public MedicalRecordId Id { get; private set; } = null!;
    public PatientId PatientId { get; private set; } = null!;
    public MedicalRecordStatus Status { get; private set; }

    public IReadOnlyList<Allergy> Allergies => _allergies.AsReadOnly();
    public IReadOnlyList<IDomainEvent> UnpublishedEvents => _unpublishedEvents.AsReadOnly();

    private MedicalRecord() { }

    public static MedicalRecord Create(MedicalRecordId id, PatientId patientId)
    {
        var medicalRecord = new MedicalRecord();
        medicalRecord.Raise(new MedicalRecordCreated(id, patientId, DateTime.UtcNow));
        return medicalRecord;
    }

    public static MedicalRecord Reconstitute(IEnumerable<IDomainEvent> events)
    {
        var medicalRecord = new MedicalRecord();
        foreach (var @event in events)
            medicalRecord.Apply(@event);
        return medicalRecord;
    }

    public void RegisterAllergy(Allergy allergy, DoctorId doctorId)
    {
        if (Status == MedicalRecordStatus.Closed)
            throw new DomainException("Cannot modify a closed medical record.");

        if (_allergies.Contains(allergy))
            throw new DomainException($"Allergy to '{allergy.Substance}' is already registered.");

        Raise(new AllergyRegistered(Id, doctorId, allergy, DateTime.UtcNow));
    }

    public void Close(MedicalRecordClosureReason reason)
    {
        if (Status == MedicalRecordStatus.Closed)
            throw new DomainException("Medical record is already closed.");

        Raise(new MedicalRecordClosed(Id, reason, DateTime.UtcNow));
    }

    public bool HasAllergyTo(Medication medication) =>
        _allergies.Any(allergy => allergy.IsAllergicTo(medication));


    #region Event Sourcing

    private void Raise(IDomainEvent @event)
    {
        Apply(@event);
        _unpublishedEvents.Add(@event);
    }

    private void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case MedicalRecordCreated e: Apply(e); break;
            case AllergyRegistered e: Apply(e); break;
            case MedicalRecordClosed e: Apply(e); break;
            default:
                throw new DomainException($"Unsupported event: {@event.GetType().Name}");
        }
    }

    private void Apply(MedicalRecordCreated e)
    {
        Id = e.MedicalRecordId;
        PatientId = e.PatientId;
        Status = MedicalRecordStatus.Active;
    }

    private void Apply(AllergyRegistered e)
    {
        Id = e.MedicalRecordId;
        _allergies.Add(e.Allergy);
    }

    private void Apply(MedicalRecordClosed e)
    {
        Id = e.MedicalRecordId;
        Status = MedicalRecordStatus.Closed;
    }

    #endregion

    public void ClearUnpublishedEvents() => _unpublishedEvents.Clear();
}