using Devclinic.MedicalRecords.Api.Endpoints;
using Devclinic.MedicalRecords.Application.Features.CloseAttendance;
using Devclinic.MedicalRecords.Application.Features.CloseMedicalRecord;
using Devclinic.MedicalRecords.Application.Features.CreateMedicalRecord;
using Devclinic.MedicalRecords.Application.Features.IssuePrescription;
using Devclinic.MedicalRecords.Application.Features.RegisterAllergy;
using Devclinic.MedicalRecords.Application.Features.RegisterChiefComplaint;
using Devclinic.MedicalRecords.Application.Features.RegisterDiagnosis;
using Devclinic.MedicalRecords.Application.Features.StartAttendance;
using Devclinic.MedicalRecords.Infrastructure;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddTransient<CreateMedicalRecordUseCase>();
builder.Services.AddTransient<CloseMedicalRecordUseCase>();
builder.Services.AddTransient<RegisterAllergyUseCase>();
builder.Services.AddTransient<StartAttendanceUseCase>();
builder.Services.AddTransient<RegisterChiefComplaintUseCase>();
builder.Services.AddTransient<RegisterDiagnosisUseCase>();
builder.Services.AddTransient<IssuePrescriptionUseCase>();
builder.Services.AddTransient<CloseAttendanceUseCase>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.UseExceptionHandler();
app.MapMedicalRecordsEndpoints();
app.MapAttendancesEndpoints();

await app.RunAsync();

public partial class Program;
