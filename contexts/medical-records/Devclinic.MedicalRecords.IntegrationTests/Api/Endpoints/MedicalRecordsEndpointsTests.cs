using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Devclinic.MedicalRecords.IntegrationTests.Api.Endpoints;

public sealed class MedicalRecordsEndpointsTests(MedicalRecordsWebApplicationFactory factory)
    : IClassFixture<MedicalRecordsWebApplicationFactory>
{
    [Fact]
    public async Task MedicalRecordWorkflow_ShouldExposeUseCasesThroughHttpContract()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var patientId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync(
            "/medical-records",
            new { patientId },
            ct);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createJson = await JsonDocument.ParseAsync(
            await createResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        var medicalRecordId = createJson.RootElement.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, medicalRecordId);
        Assert.Equal($"/medical-records/{medicalRecordId}", createResponse.Headers.Location?.OriginalString);

        var allergyResponse = await client.PostAsJsonAsync(
            $"/medical-records/{medicalRecordId}/allergies",
            new { substance = "Dipyrone", severity = "Severe" },
            ct);
        Assert.Equal(HttpStatusCode.NoContent, allergyResponse.StatusCode);

        var closeResponse = await client.PostAsJsonAsync(
            $"/medical-records/{medicalRecordId}/close",
            new { reason = "TransferRequested" },
            ct);
        Assert.Equal(HttpStatusCode.NoContent, closeResponse.StatusCode);
    }

    [Fact]
    public async Task CreateMedicalRecord_WhenActiveMedicalRecordAlreadyExists_ShouldReturnConflictProblem()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var patientId = Guid.NewGuid();

        var firstResponse = await client.PostAsJsonAsync(
            "/medical-records",
            new { patientId },
            ct);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var conflictResponse = await client.PostAsJsonAsync(
            "/medical-records",
            new { patientId },
            ct);

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        Assert.Equal("application/problem+json", conflictResponse.Content.Headers.ContentType?.MediaType);

        var problem = await JsonDocument.ParseAsync(
            await conflictResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        Assert.Contains(
            "already exists",
            problem.RootElement.GetProperty("detail").GetString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAllergy_WhenMedicalRecordDoesNotExist_ShouldReturnNotFoundProblem()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var medicalRecordId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync(
            $"/medical-records/{medicalRecordId}/allergies",
            new { substance = "Dipyrone", severity = "Severe" },
            ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RegisterAllergy_WhenSeverityIsInvalid_ShouldReturnBadRequestProblem()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;

        var response = await client.PostAsJsonAsync(
            $"/medical-records/{Guid.NewGuid()}/allergies",
            new { substance = "Dipyrone", severity = "Critical" },
            ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}