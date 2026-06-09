using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;

public sealed record AllergyRegistered(
    MedicalRecordId MedicalRecordId,
    Allergy Allergy,
    DateTime OccurredAt) : IDomainEvent;