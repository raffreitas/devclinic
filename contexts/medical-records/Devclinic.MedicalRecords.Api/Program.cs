using Devclinic.MedicalRecords.Infrastructure;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.UseExceptionHandler();

app.MapGet("/", () => "Hello World!");

await app.RunAsync();