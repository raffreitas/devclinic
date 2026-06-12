using Devclinic.MedicalRecords.Api.Endpoints;
using Devclinic.MedicalRecords.Application.Features.CloseMedicalRecord;
using Devclinic.MedicalRecords.Application.Features.CreateMedicalRecord;
using Devclinic.MedicalRecords.Application.Features.RegisterAllergy;
using Devclinic.MedicalRecords.Infrastructure;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddTransient<CreateMedicalRecordUseCase>();
builder.Services.AddTransient<CloseMedicalRecordUseCase>();
builder.Services.AddTransient<RegisterAllergyUseCase>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.UseExceptionHandler();
app.MapMedicalRecordsEndpoints();

await app.RunAsync();

public partial class Program;
