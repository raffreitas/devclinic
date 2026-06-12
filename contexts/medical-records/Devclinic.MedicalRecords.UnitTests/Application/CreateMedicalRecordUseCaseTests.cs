using Devclinic.MedicalRecords.Application.Abstractions;
using Devclinic.MedicalRecords.Application.Features.CreateMedicalRecord;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Shared.ValueObjects;

using NSubstitute;

namespace Devclinic.MedicalRecords.UnitTests.Application;

public sealed class CreateMedicalRecordUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCommandIsValid_ShouldCreateAndSaveMedicalRecord()
    {
        var patientService = Substitute.For<IPatientService>();
        var repository = Substitute.For<IMedicalRecordRepository>();
        var command = new CreateMedicalRecordCommand(Guid.NewGuid());
        patientService.ExistsAsync(Arg.Any<PatientId>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.ExistsByPatientIdAsync(Arg.Any<PatientId>(), Arg.Any<CancellationToken>()).Returns(false);
        var useCase = new CreateMedicalRecordUseCase(patientService, repository);

        var medicalRecordId = await useCase.ExecuteAsync(command, TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, medicalRecordId.Value);
        await repository.Received(1).SaveAsync(
            Arg.Is<MedicalRecord>(medicalRecord => medicalRecord.PatientId.Value == command.PatientId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPatientDoesNotExist_ShouldThrowInvalidOperationException()
    {
        var patientService = Substitute.For<IPatientService>();
        var repository = Substitute.For<IMedicalRecordRepository>();
        var command = new CreateMedicalRecordCommand(Guid.NewGuid());
        patientService.ExistsAsync(Arg.Any<PatientId>(), Arg.Any<CancellationToken>()).Returns(false);
        var useCase = new CreateMedicalRecordUseCase(patientService, repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(command, TestContext.Current.CancellationToken));

        Assert.Equal("The patient does not exist.", exception.Message);
        await repository.DidNotReceive().SaveAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMedicalRecordAlreadyExists_ShouldThrowInvalidOperationException()
    {
        var patientService = Substitute.For<IPatientService>();
        var repository = Substitute.For<IMedicalRecordRepository>();
        var command = new CreateMedicalRecordCommand(Guid.NewGuid());
        patientService.ExistsAsync(Arg.Any<PatientId>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.ExistsByPatientIdAsync(Arg.Any<PatientId>(), Arg.Any<CancellationToken>()).Returns(true);
        var useCase = new CreateMedicalRecordUseCase(patientService, repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(command, TestContext.Current.CancellationToken));

        Assert.Equal("The medical record already exists.", exception.Message);
        await repository.DidNotReceive().SaveAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
    }
}
