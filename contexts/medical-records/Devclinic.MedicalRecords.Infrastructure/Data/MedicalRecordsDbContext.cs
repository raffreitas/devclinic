using Devclinic.MedicalRecords.Infrastructure.Data.Models;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devclinic.MedicalRecords.Infrastructure.Data;

public sealed class MedicalRecordsDbContext(DbContextOptions<MedicalRecordsDbContext> options) : DbContext(options)
{
    public DbSet<StoredMedicalRecordEvent> MedicalRecordEvents => Set<StoredMedicalRecordEvent>();
    public DbSet<MedicalRecordIndex> MedicalRecordIndexes => Set<MedicalRecordIndex>();

    public DbSet<StoredAttendanceEvent> AttendanceEvents => Set<StoredAttendanceEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredMedicalRecordEvent>(builder
            => MapStoredEvent(builder, "MedicalRecordEvents"));

        modelBuilder.Entity<StoredAttendanceEvent>(builder
            => MapStoredEvent(builder, "AttendanceEvents"));

        modelBuilder.Entity<MedicalRecordIndex>(MapMedicalRecordIndex);
    }

    private static void MapStoredEvent<T>(EntityTypeBuilder<T> builder, string tableName) where T : StoredEvent
    {
        builder.ToTable(tableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.AggregateId)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("JSON")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired();

        builder.HasIndex(x => new { x.AggregateId, x.Version }).IsUnique();
    }

    private static void MapMedicalRecordIndex(EntityTypeBuilder<MedicalRecordIndex> builder)
    {
        builder.ToTable("MedicalRecordIndexes");

        builder.HasKey(x => x.MedicalRecordId);

        builder.HasIndex(x => x.PatientId)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasMaxLength(32)
            .IsRequired();
    }
}