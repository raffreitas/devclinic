using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;

namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;

public sealed record ChiefComplaintRegistered(ChiefComplaint ChiefComplaint, DateTime OccurredAt) : IDomainEvent;