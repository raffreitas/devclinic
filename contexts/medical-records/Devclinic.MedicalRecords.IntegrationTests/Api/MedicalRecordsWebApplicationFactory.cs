using Devclinic.MedicalRecords.Infrastructure.Data;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.MsSql;

namespace Devclinic.MedicalRecords.IntegrationTests.Api;

public sealed class MedicalRecordsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"medical-records-db-{Guid.NewGuid():N}";

    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:latest")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.AddDbContext<MedicalRecordsDbContext>(options => options.UseSqlServer(GetMssqlConnectionString()));
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

    private MedicalRecordsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseSqlServer(GetMssqlConnectionString())
            .Options;

        return new MedicalRecordsDbContext(options);
    }

    private string GetMssqlConnectionString()
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString())
        {
            InitialCatalog = _databaseName,
        };

        return connectionStringBuilder.ConnectionString;
    }
}