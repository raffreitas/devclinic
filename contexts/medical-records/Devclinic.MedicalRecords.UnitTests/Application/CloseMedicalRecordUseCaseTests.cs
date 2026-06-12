using Devclinic.MedicalRecords.Application.Features.CloseMedicalRecord;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Enums;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Devclinic.MedicalRecords.UnitTests.Application;

public sealed class CloseMedicalRecordUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordExists_ShouldCloseAndSaveMedicalRecord()
    {
        var repository = Substitute.For<IMedicalRecordRepository>();
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.ClearUnpublishedEvents();
        repository.GetByIdAsync(medicalRecord.Id, Arg.Any<CancellationToken>()).Returns(medicalRecord);
        var useCase = new CloseMedicalRecordUseCase(repository);

        await useCase.ExecuteAsync(
            new CloseMedicalRecordCommand(medicalRecord.Id.Value, "TransferRequested"),
            TestContext.Current.CancellationToken);

        Assert.Equal(MedicalRecordStatus.Closed, medicalRecord.Status);
        await repository.Received(1).SaveAsync(medicalRecord, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordDoesNotExist_ShouldThrowArgumentException()
    {
        var repository = Substitute.For<IMedicalRecordRepository>();
        var medicalRecordId = Guid.NewGuid();
        repository.GetByIdAsync(new MedicalRecordId(medicalRecordId), Arg.Any<CancellationToken>())
            .ReturnsNull();
        var useCase = new CloseMedicalRecordUseCase(repository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(
                new CloseMedicalRecordCommand(medicalRecordId, "TransferRequested"),
                TestContext.Current.CancellationToken));

        Assert.Equal($"MedicalRecord with id {medicalRecordId} not found", exception.Message);
        await repository.DidNotReceive().SaveAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordIsAlreadyClosed_ShouldThrowDomainException()
    {
        var repository = Substitute.For<IMedicalRecordRepository>();
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.Close(Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events.MedicalRecordClosureReason.TransferRequested);
        medicalRecord.ClearUnpublishedEvents();
        repository.GetByIdAsync(medicalRecord.Id, Arg.Any<CancellationToken>()).Returns(medicalRecord);
        var useCase = new CloseMedicalRecordUseCase(repository);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            useCase.ExecuteAsync(
                new CloseMedicalRecordCommand(medicalRecord.Id.Value, "TransferRequested"),
                TestContext.Current.CancellationToken));

        Assert.Equal("Medical record is already closed.", exception.Message);
        await repository.DidNotReceive().SaveAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
    }

    private static MedicalRecord CreateMedicalRecord() =>
        MedicalRecord.Create(
            new MedicalRecordId(Guid.NewGuid()),
            new PatientId(Guid.NewGuid()));
}
