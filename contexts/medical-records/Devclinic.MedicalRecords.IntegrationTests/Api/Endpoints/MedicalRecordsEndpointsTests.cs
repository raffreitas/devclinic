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
    public async Task AttendanceWorkflow_ShouldExposeUseCasesThroughHttpContract()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var doctorId = Guid.NewGuid();
        var medicalRecordId = await CreateMedicalRecordAsync(client, ct);

        var startResponse = await client.PostAsJsonAsync(
            "/attendances",
            new
            {
                medicalRecordId,
                appointmentId = Guid.NewGuid(),
                doctorId
            },
            ct);

        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);

        var startJson = await JsonDocument.ParseAsync(
            await startResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        var attendanceId = startJson.RootElement.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, attendanceId);
        Assert.Equal($"/attendances/{attendanceId}", startResponse.Headers.Location?.OriginalString);

        var chiefComplaintResponse = await client.PostAsJsonAsync(
            $"/attendances/{attendanceId}/chief-complaint",
            new { doctorId, description = "Headache" },
            ct);
        Assert.Equal(HttpStatusCode.NoContent, chiefComplaintResponse.StatusCode);

        var diagnosisResponse = await client.PostAsJsonAsync(
            $"/attendances/{attendanceId}/diagnoses",
            new { doctorId, cid = "R51", description = "Headache", type = "Definitive" },
            ct);
        Assert.Equal(HttpStatusCode.NoContent, diagnosisResponse.StatusCode);

        var prescriptionResponse = await client.PostAsJsonAsync(
            $"/attendances/{attendanceId}/prescriptions",
            new
            {
                doctorId,
                medicationName = "Tylenol",
                activeSubstance = "Paracetamol",
                amount = "750mg",
                frequency = "8/8h",
                duration = "3 days"
            },
            ct);
        Assert.Equal(HttpStatusCode.NoContent, prescriptionResponse.StatusCode);

        var closeResponse = await client.PostAsJsonAsync(
            $"/attendances/{attendanceId}/close",
            new { doctorId },
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

    [Fact]
    public async Task RegisterDiagnosis_WhenTypeIsInvalid_ShouldReturnBadRequestProblem()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;

        var response = await client.PostAsJsonAsync(
            $"/attendances/{Guid.NewGuid()}/diagnoses",
            new
            {
                doctorId = Guid.NewGuid(),
                cid = "R51",
                description = "Headache",
                type = "Final"
            },
            ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RegisterChiefComplaint_WhenAttendanceDoesNotExist_ShouldReturnNotFoundProblem()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;

        var response = await client.PostAsJsonAsync(
            $"/attendances/{Guid.NewGuid()}/chief-complaint",
            new { doctorId = Guid.NewGuid(), description = "Headache" },
            ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CloseAttendance_WhenNoDiagnosisExists_ShouldReturnConflictProblem()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var doctorId = Guid.NewGuid();
        var attendanceId = await StartAttendanceAsync(client, doctorId, ct);

        var response = await client.PostAsJsonAsync(
            $"/attendances/{attendanceId}/close",
            new { doctorId },
            ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task IssuePrescription_WhenMedicalRecordHasAllergy_ShouldReturnConflictProblem()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var doctorId = Guid.NewGuid();
        var medicalRecordId = await CreateMedicalRecordAsync(client, ct);

        var allergyResponse = await client.PostAsJsonAsync(
            $"/medical-records/{medicalRecordId}/allergies",
            new { substance = "Dipyrone", severity = "Severe" },
            ct);
        Assert.Equal(HttpStatusCode.NoContent, allergyResponse.StatusCode);

        var attendanceId = await StartAttendanceAsync(client, medicalRecordId, doctorId, ct);

        var response = await client.PostAsJsonAsync(
            $"/attendances/{attendanceId}/prescriptions",
            new
            {
                doctorId,
                medicationName = "Novalgina",
                activeSubstance = "Dipyrone",
                amount = "500mg",
                frequency = "8/8h",
                duration = "5 days"
            },
            ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private static async Task<Guid> StartAttendanceAsync(HttpClient client, Guid doctorId, CancellationToken ct)
    {
        var medicalRecordId = await CreateMedicalRecordAsync(client, ct);

        return await StartAttendanceAsync(client, medicalRecordId, doctorId, ct);
    }

    private static async Task<Guid> StartAttendanceAsync(
        HttpClient client,
        Guid medicalRecordId,
        Guid doctorId,
        CancellationToken ct)
    {
        var startResponse = await client.PostAsJsonAsync(
            "/attendances",
            new
            {
                medicalRecordId,
                appointmentId = Guid.NewGuid(),
                doctorId
            },
            ct);
        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);

        var startJson = await JsonDocument.ParseAsync(
            await startResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);

        return startJson.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateMedicalRecordAsync(HttpClient client, CancellationToken ct)
    {
        var createResponse = await client.PostAsJsonAsync(
            "/medical-records",
            new { patientId = Guid.NewGuid() },
            ct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createJson = await JsonDocument.ParseAsync(
            await createResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);

        return createJson.RootElement.GetProperty("id").GetGuid();
    }
}
