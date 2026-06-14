using Devclinic.MedicalRecords.Application.Features.CloseAttendance;
using Devclinic.MedicalRecords.Application.Features.IssuePrescription;
using Devclinic.MedicalRecords.Application.Features.RegisterChiefComplaint;
using Devclinic.MedicalRecords.Application.Features.RegisterDiagnosis;
using Devclinic.MedicalRecords.Application.Features.StartAttendance;
using Devclinic.MedicalRecords.Domain.Aggregates.Attendances.ValueObjects;
using Devclinic.MedicalRecords.Domain.SeedWork;

namespace Devclinic.MedicalRecords.Api.Endpoints;

public static class AttendancesEndpoints
{
    public static RouteGroupBuilder MapAttendancesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/attendances")
            .WithTags("Attendances");

        group.MapPost("/", StartAttendanceAsync)
            .WithName("StartAttendance")
            .Produces<StartAttendanceResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/chief-complaint", RegisterChiefComplaintAsync)
            .WithName("RegisterChiefComplaint")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/diagnoses", RegisterDiagnosisAsync)
            .WithName("RegisterDiagnosis")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/prescriptions", IssuePrescriptionAsync)
            .WithName("IssuePrescription")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/close", CloseAttendanceAsync)
            .WithName("CloseAttendance")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> StartAttendanceAsync(
        StartAttendanceRequest request,
        StartAttendanceUseCase useCase,
        CancellationToken ct)
    {
        if (request.MedicalRecordId == Guid.Empty)
            return BadRequest("MedicalRecordId is required.");

        if (request.AppointmentId == Guid.Empty)
            return BadRequest("AppointmentId is required.");

        if (request.DoctorId == Guid.Empty)
            return BadRequest("DoctorId is required.");

        return await ExecuteCommandAsync(async () =>
        {
            var id = await useCase.ExecuteAsync(
                new StartAttendanceCommand(request.MedicalRecordId, request.AppointmentId, request.DoctorId),
                ct);

            return TypedResults.Created(
                $"/attendances/{id.Value}",
                new StartAttendanceResponse(id.Value));
        });
    }

    private static Task<IResult> RegisterChiefComplaintAsync(
        Guid id,
        RegisterChiefComplaintRequest request,
        RegisterChiefComplaintUseCase useCase,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            return Task.FromResult(BadRequest("AttendanceId is required."));

        if (request.DoctorId == Guid.Empty)
            return Task.FromResult(BadRequest("DoctorId is required."));

        if (string.IsNullOrWhiteSpace(request.Description))
            return Task.FromResult(BadRequest("Description is required."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(
                new RegisterChiefComplaintCommand(id, request.DoctorId, request.Description),
                ct);

            return TypedResults.NoContent();
        });
    }

    private static Task<IResult> RegisterDiagnosisAsync(
        Guid id,
        RegisterDiagnosisRequest request,
        RegisterDiagnosisUseCase useCase,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            return Task.FromResult(BadRequest("AttendanceId is required."));

        if (request.DoctorId == Guid.Empty)
            return Task.FromResult(BadRequest("DoctorId is required."));

        if (string.IsNullOrWhiteSpace(request.Cid))
            return Task.FromResult(BadRequest("Cid is required."));

        if (string.IsNullOrWhiteSpace(request.Description))
            return Task.FromResult(BadRequest("Description is required."));

        if (!Enum.TryParse<DiagnosisType>(request.Type, ignoreCase: true, out _))
            return Task.FromResult(BadRequest("Type is invalid."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(
                new RegisterDiagnosisCommand(id, request.DoctorId, request.Cid, request.Description, request.Type),
                ct);

            return TypedResults.NoContent();
        });
    }

    private static Task<IResult> IssuePrescriptionAsync(
        Guid id,
        IssuePrescriptionRequest request,
        IssuePrescriptionUseCase useCase,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            return Task.FromResult(BadRequest("AttendanceId is required."));

        if (request.DoctorId == Guid.Empty)
            return Task.FromResult(BadRequest("DoctorId is required."));

        if (string.IsNullOrWhiteSpace(request.MedicationName))
            return Task.FromResult(BadRequest("MedicationName is required."));

        if (string.IsNullOrWhiteSpace(request.ActiveSubstance))
            return Task.FromResult(BadRequest("ActiveSubstance is required."));

        if (string.IsNullOrWhiteSpace(request.Amount))
            return Task.FromResult(BadRequest("Amount is required."));

        if (string.IsNullOrWhiteSpace(request.Frequency))
            return Task.FromResult(BadRequest("Frequency is required."));

        if (string.IsNullOrWhiteSpace(request.Duration))
            return Task.FromResult(BadRequest("Duration is required."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(
                new IssuePrescriptionCommand(
                    id,
                    request.DoctorId,
                    request.MedicationName,
                    request.ActiveSubstance,
                    request.Amount,
                    request.Frequency,
                    request.Duration),
                ct);

            return TypedResults.NoContent();
        });
    }

    private static Task<IResult> CloseAttendanceAsync(
        Guid id,
        CloseAttendanceRequest request,
        CloseAttendanceUseCase useCase,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            return Task.FromResult(BadRequest("AttendanceId is required."));

        if (request.DoctorId == Guid.Empty)
            return Task.FromResult(BadRequest("DoctorId is required."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(new CloseAttendanceCommand(id, request.DoctorId), ct);

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
                title: "Attendance not found");
        }
        catch (InvalidOperationException exception) when (IsNotFound(exception))
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Attendance not found");
        }
        catch (InvalidOperationException exception)
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Attendance command rejected");
        }
        catch (DomainException exception)
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Attendance command rejected");
        }
    }

    private static IResult BadRequest(string detail) =>
        TypedResults.Problem(
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid attendance request");

    private static bool IsNotFound(Exception exception) =>
        exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
        exception.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
}

public sealed record StartAttendanceRequest(Guid MedicalRecordId, Guid AppointmentId, Guid DoctorId);

public sealed record StartAttendanceResponse(Guid Id);

public sealed record RegisterChiefComplaintRequest(Guid DoctorId, string Description);

public sealed record RegisterDiagnosisRequest(Guid DoctorId, string Cid, string Description, string Type);

public sealed record IssuePrescriptionRequest(
    Guid DoctorId,
    string MedicationName,
    string ActiveSubstance,
    string Amount,
    string Frequency,
    string Duration);

public sealed record CloseAttendanceRequest(Guid DoctorId);
