using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Entities;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances;

public sealed class Attendance
{
    private readonly List<IDomainEvent> _unpublishedEvents = [];
    private readonly List<Diagnosis> _diagnoses = [];
    private readonly List<Prescription> _prescriptions = [];

    public AttendanceId Id { get; private set; } = null!;
    public MedicalRecordId MedicalRecordId { get; private set; } = null!;
    public AppointmentId AppointmentId { get; private set; } = null!;
    public DoctorId DoctorId { get; private set; } = null!;
    public AttendanceStatus Status { get; private set; }
    public ChiefComplaint? ChiefComplaint { get; private set; }
    public DoctorId? ClosedBy { get; private set; }

    public IReadOnlyList<Diagnosis> Diagnoses => _diagnoses.AsReadOnly();
    public IReadOnlyList<Prescription> Prescriptions => _prescriptions.AsReadOnly();
    public IReadOnlyList<IDomainEvent> UnpublishedEvents => _unpublishedEvents.AsReadOnly();

    private Attendance() { }

    public static Attendance Start(
        AttendanceId id,
        MedicalRecordId medicalRecordId,
        AppointmentId appointmentId,
        DoctorId doctorId)
    {
        var attendance = new Attendance();
        attendance.Raise(new AttendanceStarted(id, medicalRecordId, appointmentId, doctorId, DateTime.UtcNow));
        return attendance;
    }

    public static Attendance Reconstitute(IEnumerable<IDomainEvent> events)
    {
        var attendance = new Attendance();
        foreach (var @event in events)
            attendance.Apply(@event);
        return attendance;
    }

    public void RegisterChiefComplaint(DoctorId doctorId, ChiefComplaint chiefComplaint)
    {
        EnsureInProgress();
        EnsureResponsibleDoctor(doctorId);

        Raise(new ChiefComplaintRegistered(Id, chiefComplaint, DateTime.UtcNow));
    }

    public void RegisterDiagnosis(DoctorId doctorId, Diagnosis diagnosis)
    {
        EnsureInProgress();
        EnsureResponsibleDoctor(doctorId);

        Raise(new DiagnosisRegistered(Id, diagnosis, DateTime.UtcNow));
    }

    public void IssuePrescription(DoctorId doctorId, Medication medication, Dosage dosage)
    {
        EnsureInProgress();
        EnsureResponsibleDoctor(doctorId);

        var prescriptionId = new PrescriptionId(Guid.NewGuid());
        Raise(new PrescriptionIssued(Id, prescriptionId, medication, dosage, DateTime.UtcNow));
    }

    public void Close(DoctorId doctorId)
    {
        EnsureInProgress();
        EnsureResponsibleDoctor(doctorId);

        if (_diagnoses.Count == 0)
            throw new DomainException("At least one diagnosis is required before closing the attendance.");

        Raise(new AttendanceClosed(Id, doctorId, DateTime.UtcNow));
    }

    private void EnsureInProgress()
    {
        if (Status == AttendanceStatus.Closed)
            throw new DomainException("Cannot modify a closed attendance.");
    }

    private void EnsureResponsibleDoctor(DoctorId doctorId)
    {
        if (DoctorId != doctorId)
            throw new DomainException("Only the responsible doctor can modify this attendance.");
    }

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
            case AttendanceStarted e: Apply(e); break;
            case ChiefComplaintRegistered e: Apply(e); break;
            case DiagnosisRegistered e: Apply(e); break;
            case PrescriptionIssued e: Apply(e); break;
            case AttendanceClosed e: Apply(e); break;
            default:
                throw new DomainException($"Unsupported event: {@event.GetType().Name}");
        }
    }

    private void Apply(AttendanceStarted e)
    {
        Id = e.AttendanceId;
        MedicalRecordId = e.MedicalRecordId;
        AppointmentId = e.AppointmentId;
        DoctorId = e.DoctorId;
        Status = AttendanceStatus.InProgress;
    }

    private void Apply(ChiefComplaintRegistered e)
    {
        Id = e.AttendanceId;
        ChiefComplaint = e.ChiefComplaint;
    }

    private void Apply(DiagnosisRegistered e)
    {
        Id = e.AttendanceId;
        _diagnoses.Add(e.Diagnosis);
    }

    private void Apply(PrescriptionIssued e)
    {
        Id = e.AttendanceId;
        _prescriptions.Add(new Prescription(e.PrescriptionId, e.Medication, e.Dosage));
    }

    private void Apply(AttendanceClosed e)
    {
        Id = e.AttendanceId;
        ClosedBy = e.ClosedBy;
        Status = AttendanceStatus.Closed;
    }

    #endregion

    public void ClearUnpublishedEvents() => _unpublishedEvents.Clear();
}