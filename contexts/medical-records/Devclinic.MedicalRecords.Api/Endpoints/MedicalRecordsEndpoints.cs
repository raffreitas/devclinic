using Devclinic.MedicalRecords.Application.Features.CloseMedicalRecord;
using Devclinic.MedicalRecords.Application.Features.CreateMedicalRecord;
using Devclinic.MedicalRecords.Application.Features.RegisterAllergy;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.Events;
using Devclinic.MedicalRecords.Domain.Aggregates.MedicalRecords.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;

namespace Devclinic.MedicalRecords.Api.Endpoints;

public static class MedicalRecordsEndpoints
{
    public static RouteGroupBuilder MapMedicalRecordsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/medical-records")
            .WithTags("Medical Records");

        group.MapPost("/", CreateMedicalRecordAsync)
            .WithName("CreateMedicalRecord")
            .Produces<CreateMedicalRecordResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/allergies", RegisterAllergyAsync)
            .WithName("RegisterAllergy")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/close", CloseMedicalRecordAsync)
            .WithName("CloseMedicalRecord")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> CreateMedicalRecordAsync(
        CreateMedicalRecordRequest request,
        CreateMedicalRecordUseCase useCase,
        CancellationToken ct)
    {
        if (request.PatientId == Guid.Empty)
            return BadRequest("PatientId is required.");

        return await ExecuteCommandAsync(async () =>
        {
            var id = await useCase.ExecuteAsync(
                new CreateMedicalRecordCommand(request.PatientId),
                ct);

            return TypedResults.Created(
                $"/medical-records/{id.Value}",
                new CreateMedicalRecordResponse(id.Value));
        });
    }

    private static Task<IResult> RegisterAllergyAsync(
        Guid id,
        RegisterAllergyRequest request,
        RegisterAllergyUseCase useCase,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            return Task.FromResult(BadRequest("MedicalRecordId is required."));

        if (string.IsNullOrWhiteSpace(request.Substance))
            return Task.FromResult(BadRequest("Substance is required."));

        if (!Enum.TryParse<AllergySeverity>(request.Severity, ignoreCase: true, out _))
            return Task.FromResult(BadRequest("Severity is invalid."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(
                new RegisterAllergyCommand(id, request.Substance, request.Severity),
                ct);

            return TypedResults.NoContent();
        });
    }

    private static Task<IResult> CloseMedicalRecordAsync(
        Guid id,
        CloseMedicalRecordRequest request,
        CloseMedicalRecordUseCase useCase,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            return Task.FromResult(BadRequest("MedicalRecordId is required."));

        if (!Enum.TryParse<MedicalRecordClosureReason>(request.Reason, ignoreCase: true, out _))
            return Task.FromResult(BadRequest("Reason is invalid."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(
                new CloseMedicalRecordCommand(id, request.Reason),
                ct);

            return TypedResults.NoContent();
        });
    }

    private static async Task<IResult> ExecuteCommandAsync(Func<Task<IResult>> command)
    {
        try
        {
            return await command();
        }
        catch (ArgumentException exception) when (IsNotFound(exception))
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Medical record not found");
        }
        catch (InvalidOperationException exception) when (IsNotFound(exception))
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Medical record not found");
        }
        catch (InvalidOperationException exception)
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Medical record command rejected");
        }
        catch (DomainException exception)
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Medical record command rejected");
        }
    }

    private static IResult BadRequest(string detail) =>
        TypedResults.Problem(
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid medical record request");

    private static bool IsNotFound(Exception exception) =>
        exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
        exception.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
}

public sealed record CreateMedicalRecordRequest(Guid PatientId);

public sealed record CreateMedicalRecordResponse(Guid Id);

public sealed record RegisterAllergyRequest(string Substance, string Severity);

public sealed record CloseMedicalRecordRequest(string Reason);
