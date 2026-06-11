using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;

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