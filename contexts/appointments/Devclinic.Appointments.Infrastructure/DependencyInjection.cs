using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Domain.Abstractions;
using Devclinic.Appointments.Infrastructure.Data;
using Devclinic.Appointments.Infrastructure.Data.Queries;
using Devclinic.Appointments.Infrastructure.Data.Repositories;
using Devclinic.Appointments.Infrastructure.Messaging;
using Devclinic.Appointments.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devclinic.Appointments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAppointmentsInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextPool<AppointmentsDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DatabaseConnection"));
        });

        services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();
        services.AddScoped<IAppointmentQueries, EfAppointmentQueries>();
        services.AddSingleton<IDoctorService, InMemoryDoctorService>();
        services.AddSingleton<IPatientService, InMemoryPatientService>();
        services.AddSingleton<ChannelEventBus>();
        services.AddSingleton<IEventBus>(provider => provider.GetRequiredService<ChannelEventBus>());

        return services;
    }
}