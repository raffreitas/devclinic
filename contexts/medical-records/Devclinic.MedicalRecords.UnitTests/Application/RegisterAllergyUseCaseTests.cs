using Devclinic.MedicalRecords.Application.Abstractions;
using Devclinic.MedicalRecords.Application.Features.RegisterAllergy;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Devclinic.MedicalRecords.UnitTests.Application;

public sealed class RegisterAllergyUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordExistsAndCanWrite_ShouldRegisterAllergyAndSave()
    {
        var repository = Substitute.For<IMedicalRecordRepository>();
        var accessService = Substitute.For<IMedicalRecordAccessService>();
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.ClearUnpublishedEvents();
        repository.GetByIdAsync(medicalRecord.Id, Arg.Any<CancellationToken>()).Returns(medicalRecord);
        accessService.CanWriteAsync(Arg.Any<DoctorId>(), medicalRecord.PatientId, Arg.Any<CancellationToken>())
            .Returns(true);
        var useCase = new RegisterAllergyUseCase(repository, accessService);

        await useCase.ExecuteAsync(
            new RegisterAllergyCommand(medicalRecord.Id.Value, "Dipyrone", "Severe"),
            TestContext.Current.CancellationToken);

        var allergy = Assert.Single(medicalRecord.Allergies);
        Assert.Equal("Dipyrone", allergy.Substance);
        Assert.Equal(AllergySeverity.Severe, allergy.Severity);
        await repository.Received(1).SaveAsync(medicalRecord, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordDoesNotExist_ShouldThrowArgumentException()
    {
        var repository = Substitute.For<IMedicalRecordRepository>();
        var accessService = Substitute.For<IMedicalRecordAccessService>();
        var medicalRecordId = Guid.NewGuid();
        repository.GetByIdAsync(new MedicalRecordId(medicalRecordId), Arg.Any<CancellationToken>())
            .ReturnsNull();
        var useCase = new RegisterAllergyUseCase(repository, accessService);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(
                new RegisterAllergyCommand(medicalRecordId, "Dipyrone", "Severe"),
                TestContext.Current.CancellationToken));

        Assert.Equal($"MedicalRecord with id {medicalRecordId} not found", exception.Message);
        await repository.DidNotReceive().SaveAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDoctorCannotWrite_ShouldThrowArgumentException()
    {
        var repository = Substitute.For<IMedicalRecordRepository>();
        var accessService = Substitute.For<IMedicalRecordAccessService>();
        var medicalRecord = CreateMedicalRecord();
        repository.GetByIdAsync(medicalRecord.Id, Arg.Any<CancellationToken>()).Returns(medicalRecord);
        accessService.CanWriteAsync(Arg.Any<DoctorId>(), medicalRecord.PatientId, Arg.Any<CancellationToken>())
            .Returns(false);
        var useCase = new RegisterAllergyUseCase(repository, accessService);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(
                new RegisterAllergyCommand(medicalRecord.Id.Value, "Dipyrone", "Severe"),
                TestContext.Current.CancellationToken));

        Assert.Equal($"MedicalRecord with id {medicalRecord.Id.Value} not found", exception.Message);
        await repository.DidNotReceive().SaveAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllergyAlreadyExists_ShouldThrowDomainException()
    {
        var repository = Substitute.For<IMedicalRecordRepository>();
        var accessService = Substitute.For<IMedicalRecordAccessService>();
        var medicalRecord = CreateMedicalRecord();
        medicalRecord.RegisterAllergy(new Allergy("Dipyrone", AllergySeverity.Severe), new DoctorId(Guid.NewGuid()));
        medicalRecord.ClearUnpublishedEvents();
        repository.GetByIdAsync(medicalRecord.Id, Arg.Any<CancellationToken>()).Returns(medicalRecord);
        accessService.CanWriteAsync(Arg.Any<DoctorId>(), medicalRecord.PatientId, Arg.Any<CancellationToken>())
            .Returns(true);
        var useCase = new RegisterAllergyUseCase(repository, accessService);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            useCase.ExecuteAsync(
                new RegisterAllergyCommand(medicalRecord.Id.Value, "Dipyrone", "Severe"),
                TestContext.Current.CancellationToken));

        Assert.Equal("Allergy to 'Dipyrone' is already registered.", exception.Message);
        await repository.DidNotReceive().SaveAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
    }

    private static MedicalRecord CreateMedicalRecord() =>
        MedicalRecord.Create(
            new MedicalRecordId(Guid.NewGuid()),
            new PatientId(Guid.NewGuid()));
}
