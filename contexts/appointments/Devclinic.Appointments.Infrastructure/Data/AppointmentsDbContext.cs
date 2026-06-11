using Devclinic.Appointments.Domain;
using Devclinic.Appointments.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

namespace Devclinic.Appointments.Infrastructure.Data;

public sealed class AppointmentsDbContext(DbContextOptions<AppointmentsDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var appointment = modelBuilder.Entity<Appointment>();

        appointment.ToTable("Appointments");
        appointment.HasKey(entity => entity.Id);

        appointment.Property(entity => entity.Id)
            .HasConversion(id => id.Value, value => new AppointmentId(value))
            .ValueGeneratedNever();

        appointment.ComplexProperty(e => e.PatientId, patientBuilder =>
        {
            patientBuilder.Property(t => t.Value)
                .HasColumnName("PatientId");
        });

        appointment.ComplexProperty(e => e.DoctorId, doctorBuilder =>
        {
            doctorBuilder.Property(t => t.Value)
                .HasColumnName("DoctorId");
        });

        appointment.ComplexProperty(e => e.Time, timeBuilder =>
        {
            timeBuilder.Property(t => t.Value)
                .HasColumnName("Time");
        });

        appointment.Property(entity => entity.CancellationReason)
            .HasMaxLength(1000)
            .HasConversion(
                reason => reason == null ? null : reason.Value,
                value => value == null ? null : new CancellationReason(value));

        appointment.Property(entity => entity.Status)
            .HasMaxLength(25)
            .HasConversion<string>();

        appointment.OwnsMany(entity => entity.StatusHistory, statusChangeBuilder =>
        {
            statusChangeBuilder.ToTable("AppointmentStatusChanges");
            statusChangeBuilder.WithOwner().HasForeignKey("AppointmentId");
            statusChangeBuilder.Property<int>("Id").ValueGeneratedOnAdd();
            statusChangeBuilder.HasKey("Id");

            statusChangeBuilder.Property(statusChange => statusChange.Status)
                .HasMaxLength(25)
                .HasConversion<string>();

            statusChangeBuilder.Property(statusChange => statusChange.OccurredAt)
                .ValueGeneratedNever();
        });

        appointment.Ignore(entity => entity.DomainEvents);
    }
}