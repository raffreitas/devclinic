using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Infrastructure.Data;
using Devclinic.MedicalRecords.Infrastructure.Data.Repositories;
using Devclinic.MedicalRecords.Infrastructure.Data.Serializers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devclinic.MedicalRecords.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<MedicalRecordsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DatabaseConnection")));

        services.AddSingleton(new MedicalRecordsEventSerializer());
        services.AddScoped<IMedicalRecordRepository, EfMedicalRecordRepository>();

        return services;
    }
}