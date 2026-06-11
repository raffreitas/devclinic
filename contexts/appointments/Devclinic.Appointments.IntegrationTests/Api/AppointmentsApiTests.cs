using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Devclinic.Appointments.Api;
using Devclinic.Appointments.Domain.Events;
using Devclinic.Appointments.Infrastructure.Data;
using Devclinic.Appointments.Infrastructure.Messaging;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.MsSql;

namespace Devclinic.Appointments.IntegrationTests.Api;

public sealed class AppointmentsApiTests(AppointmentsApiFactory factory)
    : IClassFixture<AppointmentsApiFactory>
{
    [Fact]
    public async Task AppointmentWorkflow_ShouldExposeUseCasesThroughHttpContract()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync(
            "/appointments",
            new { doctorId, patientId, time = DateTime.UtcNow.AddDays(1) },
            ct);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createJson = await JsonDocument.ParseAsync(
            await createResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        var appointmentId = createJson.RootElement.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, appointmentId);
        Assert.Equal($"/appointments/{appointmentId}", createResponse.Headers.Location?.OriginalString);

        var getResponse = await client.GetAsync($"/appointments/{appointmentId}", ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getJson = await JsonDocument.ParseAsync(
            await getResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        Assert.Equal("Scheduled", getJson.RootElement.GetProperty("status").GetString());

        var confirmResponse = await client.PostAsJsonAsync(
            $"/appointments/{appointmentId}/confirm",
            new { doctorId },
            ct);
        Assert.Equal(HttpStatusCode.NoContent, confirmResponse.StatusCode);

        var rescheduleResponse = await client.PostAsJsonAsync(
            $"/appointments/{appointmentId}/reschedule-by-patient",
            new { patientId, newTime = DateTime.UtcNow.AddDays(2) },
            ct);
        Assert.Equal(HttpStatusCode.NoContent, rescheduleResponse.StatusCode);

        var cancelResponse = await client.PostAsJsonAsync(
            $"/appointments/{appointmentId}/cancel-by-doctor",
            new { doctorId, reason = "Doctor unavailable" },
            ct);
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var finalResponse = await client.GetAsync($"/appointments/{appointmentId}", ct);
        Assert.Equal(HttpStatusCode.OK, finalResponse.StatusCode);

        var finalJson = await JsonDocument.ParseAsync(
            await finalResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        Assert.Equal("Cancelled", finalJson.RootElement.GetProperty("status").GetString());
        Assert.Equal("Doctor unavailable", finalJson.RootElement.GetProperty("cancellationReason").GetString());

        var statusChanges = finalJson.RootElement.GetProperty("statusChanges").EnumerateArray().ToArray();
        var statuses = statusChanges
            .Select(statusChange => statusChange.GetProperty("status").GetString() ?? string.Empty)
            .ToArray();

        Assert.Equal(["Scheduled", "Confirmed", "Rescheduled", "Cancelled"], statuses);
    }

    [Fact]
    public async Task ScheduleAppointment_WhenDoctorAlreadyHasAppointmentAtTime_ShouldReturnConflictProblem()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var doctorId = Guid.NewGuid();
        var firstPatientId = Guid.NewGuid();
        var secondPatientId = Guid.NewGuid();
        var appointmentTime = DateTime.UtcNow.AddDays(1);

        var firstResponse = await client.PostAsJsonAsync(
            "/appointments",
            new { doctorId, patientId = firstPatientId, time = appointmentTime },
            ct);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var conflictResponse = await client.PostAsJsonAsync(
            "/appointments",
            new { doctorId, patientId = secondPatientId, time = appointmentTime },
            ct);

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        Assert.Equal("application/problem+json", conflictResponse.Content.Headers.ContentType?.MediaType);

        var problem = await JsonDocument.ParseAsync(
            await conflictResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        Assert.Contains(
            "already has an appointment",
            problem.RootElement.GetProperty("detail").GetString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAppointment_WhenAppointmentDoesNotExist_ShouldReturnNotFound()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;

        var response = await client.GetAsync($"/appointments/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentsByDoctor_ShouldFilterByDoctorAndDateRange()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointmentTime = DateTime.UtcNow.AddDays(1);

        var createResponse = await client.PostAsJsonAsync(
            "/appointments",
            new { doctorId, patientId, time = appointmentTime },
            ct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var from = DateOnly.FromDateTime(appointmentTime.AddDays(-1));
        var to = DateOnly.FromDateTime(appointmentTime.AddDays(1));
        var listResponse =
            await client.GetAsync($"/appointments?doctorId={doctorId}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listJson = await JsonDocument.ParseAsync(
            await listResponse.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);

        Assert.Single(listJson.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task ScheduleAppointment_ShouldPublishDomainEventToChannel()
    {
        using var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;
        var eventBus = factory.Services.GetRequiredService<ChannelEventBus>();

        var createResponse = await client.PostAsJsonAsync(
            "/appointments",
            new { doctorId = Guid.NewGuid(), patientId = Guid.NewGuid(), time = DateTime.UtcNow.AddDays(1) },
            ct);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var publishedEvent = await eventBus.ReadAsync(ct);

        Assert.IsType<AppointmentScheduled>(publishedEvent);
    }
}

public sealed class AppointmentsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:latest")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.AddDbContext<AppointmentsDbContext>(options => options.UseSqlServer(GetMssqlConnectionString()));
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    private AppointmentsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppointmentsDbContext>()
            .UseSqlServer(GetMssqlConnectionString())
            .Options;

        return new AppointmentsDbContext(options);
    }

    private string GetMssqlConnectionString()
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString())
        {
            InitialCatalog = $"appointments-db-{Guid.NewGuid():N}",
        };

        return connectionStringBuilder.ConnectionString;
    }
}