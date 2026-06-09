using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;

public sealed record AttendanceStarted(
    AttendanceId AttendanceId,
    MedicalRecordId MedicalRecordId,
    AppointmentId AppointmentId,
    DoctorId DoctorId,
    DateTime OccurredAt) : IDomainEvent;