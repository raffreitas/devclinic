using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;

public sealed record AllergyRegistered(Allergy Allergy, DateTime OccurredAt) : IDomainEvent;