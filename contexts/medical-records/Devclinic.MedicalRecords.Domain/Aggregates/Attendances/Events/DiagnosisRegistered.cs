using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;

namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;

public sealed record DiagnosisRegistered(
    AttendanceId AttendanceId,
    Diagnosis Diagnosis,
    DateTime OccurredAt) : IDomainEvent;