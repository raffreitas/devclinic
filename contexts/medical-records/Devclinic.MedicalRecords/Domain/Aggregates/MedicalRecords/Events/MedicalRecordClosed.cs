using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;

public sealed record MedicalRecordClosed(
    MedicalRecordId MedicalRecordId,
    MedicalRecordClosureReason Reason,
    DateTime OccurredAt) : IDomainEvent;

public enum MedicalRecordClosureReason
{
    PatientDeceased,
    TransferRequested
}