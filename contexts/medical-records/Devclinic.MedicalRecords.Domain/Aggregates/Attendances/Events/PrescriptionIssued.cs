using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;

public sealed record PrescriptionIssued(
    AttendanceId AttendanceId,
    PrescriptionId PrescriptionId,
    Medication Medication,
    Dosage Dosage,
    DateTime OccurredAt) : IDomainEvent;