using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Events;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

namespace Devclinic.MedicalRecords.Domain.Aggregates.Attendances;

public sealed class Attendance
{
    private readonly List<IDomainEvent> _unpublishedEvents = [];


    public AttendanceId AttendanceId { get; private set; } = null!;
    public MedicalRecordId MedicalRecordId { get; private set; } = null!;
    public AppointmentId AppointmentId { get; private set; } = null!;
    public DoctorId DoctorId { get; private set; } = null!;
    public AttendanceStatus Status { get; private set; }
    public ChiefComplaint? ChiefComplaint { get; private set; }
    public Signature? Signature { get; private set; }

    public IReadOnlyList<IDomainEvent> UnpublishedEvents => _unpublishedEvents.AsReadOnly();


    private Attendance()
    {
    }

    public static Attendance Create(
        AttendanceId attendanceId,
        MedicalRecordId medicalRecordId,
        AppointmentId appointmentId,
        DoctorId doctorId)
    {
        var attendance = new Attendance();
        attendance.Raise(new AttendanceStarted(
            attendanceId,
            medicalRecordId,
            appointmentId,
            doctorId,
            DateTime.UtcNow));
        return attendance;
    }

    public void RegisterChiefComplaint(ChiefComplaint chiefComplaint)
    {
        Raise(new ChiefComplaintRegistered(chiefComplaint, DateTime.UtcNow));
    }

    public void Close(Signature signature)
    {
    }

    private void Raise(IDomainEvent @event)
    {
        Apply(@event);
        _unpublishedEvents.Add(@event);
    }

    private void Apply(IDomainEvent @event)
    {
        switch (@event)
        {
            case AttendanceStarted started:
                Apply(started);
                break;
            case ChiefComplaintRegistered chiefComplaintRegistered:
                Apply(chiefComplaintRegistered);
                break;
            // case ClinicalExamRecorded clinicalExamRecorded:
            //     Apply(clinicalExamRecorded);
            //     break;
            // case DiagnosisRegistered diagnosisRegistered:
            //     Apply(diagnosisRegistered);
            //     break;
            // case PrescriptionIssued prescriptionIssued:
            //     Apply(prescriptionIssued);
            //     break;
            // case ExamRequested examRequested:
            //     Apply(examRequested);
            //     break;
            // case CorrectionRegistered correctionRegistered:
            //     Apply(correctionRegistered);
            //     break;
            // case AttendanceClosed attendanceClosed:
            //     Apply(attendanceClosed);
                break;
            default:
                throw new DomainException($"Event not supported: {@event.GetType().Name}");
        }
    }

    private void Apply(AttendanceStarted e)
    {
        AttendanceId = e.AttendanceId;
        MedicalRecordId = e.MedicalRecordId;
        AppointmentId = e.AppointmentId;
        DoctorId = e.DoctorId;
        Status = AttendanceStatus.InProgress;
    }

    private void Apply(ChiefComplaintRegistered e)
    {
        _chiefComplaints.Add(e.ChiefComplaint);
    }


    /*
    - MedicalRecordId
    - AppointmentId
    - ChiefComplaint
    - Diagnosis
    - Prescriptions
    - RequestedExams
    - DoctorSignature
     */
}