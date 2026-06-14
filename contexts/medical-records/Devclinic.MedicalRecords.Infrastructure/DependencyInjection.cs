using Devclinic.MedicalRecords.Application.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.Abstractions;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Abstractions;
using Devclinic.MedicalRecords.Domain.Services;
using Devclinic.MedicalRecords.Infrastructure.Data;
using Devclinic.MedicalRecords.Infrastructure.Data.Repositories;
using Devclinic.MedicalRecords.Infrastructure.Data.Serializers;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Abstractions;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Handlers;
using Devclinic.MedicalRecords.Infrastructure.EventSourcing.Handlers.Common;
using Devclinic.MedicalRecords.Infrastructure.MessageBus;
using Devclinic.MedicalRecords.Infrastructure.Outbox;
using Devclinic.MedicalRecords.Infrastructure.Services;

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
        services.AddSingleton(new AttendanceEventSerializer());
        services.AddScoped<IMedicalRecordRepository, EfMedicalRecordRepository>();
        services.AddScoped<IAttendanceRepository, EfAttendanceRepository>();
        services.AddSingleton<PrescriptionService>();

        services.AddSingleton<IPatientService, InMemoryPatientService>();
        services.AddSingleton<IMedicalRecordAccessService, InMemoryMedicalRecordAccessService>();

        services.AddScoped<IProjectionDispatcher, ProjectionDispatcher>();

        services.AddScoped<
            IProjectionHandler,
            MedicalRecordCreatedProjectionHandler>();

        services.AddScoped<
            IProjectionHandler,
            MedicalRecordClosedProjectionHandler>();

        services.AddScoped<IOutboxRepository, EfOutboxRepository>();
        services.AddSingleton<IMessageBus, LoggingMessageBus>();
        services.AddHostedService<PublishOutboxMessagesWorker>();

        return services;
    }
}