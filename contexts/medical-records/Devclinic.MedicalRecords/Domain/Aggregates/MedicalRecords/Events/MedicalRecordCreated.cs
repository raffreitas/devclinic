using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;

public sealed record MedicalRecordCreated(
    MedicalRecordId MedicalRecordId,
    PatientId PatientId,
    DateTime OccurredAt) : IDomainEvent;