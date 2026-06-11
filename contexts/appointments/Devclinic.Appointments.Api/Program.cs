using Devclinic.Appointments.Api.Endpoints;
using Devclinic.Appointments.Application.Features.CancelAppointmentByDoctor;
using Devclinic.Appointments.Application.Features.CancelAppointmentByPatient;
using Devclinic.Appointments.Application.Features.ConfirmAppointment;
using Devclinic.Appointments.Application.Features.RescheduleAppointmentByDoctor;
using Devclinic.Appointments.Application.Features.RescheduleAppointmentByPatient;
using Devclinic.Appointments.Application.Features.ScheduleAppointment;
using Devclinic.Appointments.Infrastructure;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddAppointmentsInfrastructure(builder.Configuration);

builder.Services.AddTransient<ScheduleAppointmentUseCase>();
builder.Services.AddTransient<ConfirmAppointmentUseCase>();
builder.Services.AddTransient<CancelAppointmentByDoctorUseCase>();
builder.Services.AddTransient<CancelAppointmentByPatientUseCase>();
builder.Services.AddTransient<RescheduleAppointmentByDoctorUseCase>();
builder.Services.AddTransient<RescheduleAppointmentByPatientUseCase>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.UseExceptionHandler();
app.MapAppointmentsEndpoints();

await app.RunAsync();

public partial class Program;