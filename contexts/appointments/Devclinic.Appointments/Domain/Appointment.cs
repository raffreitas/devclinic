using Devclinic.Appointments.Domain.Enums;
using Devclinic.Appointments.Domain.Events;
using Devclinic.Appointments.Domain.SeedWork;
using Devclinic.Appointments.Domain.ValueObjects;

namespace Devclinic.Appointments.Domain;

public sealed class Appointment
{
    public AppointmentId Id { get; private set; }
    public PatientId PatientId { get; private set; }
    public DoctorId DoctorId { get; private set; }
    public AppointmentTime Time { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public CancellationReason? CancellationReason { get; private set; }

    private readonly List<StatusChange> _statusHistory = [];
    public IReadOnlyList<StatusChange> StatusHistory => _statusHistory.AsReadOnly();

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();


    public Appointment(
        AppointmentId id,
        PatientId patientId,
        DoctorId doctorId,
        AppointmentTime time)
    {
        Id = id;
        PatientId = patientId;
        DoctorId = doctorId;
        Time = time;
        Status = AppointmentStatus.Scheduled;

        _statusHistory.Add(new StatusChange(Status, DateTime.UtcNow));
        _domainEvents.Add(new AppointmentScheduled(id, patientId, doctorId, time));
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new DomainException("Only scheduled appointments can be confirmed.");

        Status = AppointmentStatus.Confirmed;

        _statusHistory.Add(new StatusChange(Status, DateTime.UtcNow));
        _domainEvents.Add(new AppointmentConfirmedEvent(Id, PatientId, DoctorId, Time));
    }

    public void Cancel(string cancellationReason)
    {
        if (string.IsNullOrWhiteSpace(cancellationReason))
            throw new DomainException("A cancellation reason is required.");

        Status = AppointmentStatus.Cancelled;
        CancellationReason = new CancellationReason(cancellationReason);

        _statusHistory.Add(new StatusChange(Status, DateTime.UtcNow));
        _domainEvents.Add(new AppointmentCancelledEvent(Id, PatientId, DoctorId, CancellationReason));
    }

    public void Reschedule(AppointmentTime newTime)
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new DomainException("Cancelled appointments cannot be rescheduled.");

        Status = AppointmentStatus.Rescheduled;
        Time = newTime;

        _statusHistory.Add(new StatusChange(Status, DateTime.UtcNow));
    }
}