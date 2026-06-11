using Devclinic.Appointments.Application.Abstractions;
using Devclinic.Appointments.Application.Features.CancelAppointmentByDoctor;
using Devclinic.Appointments.Application.Features.CancelAppointmentByPatient;
using Devclinic.Appointments.Application.Features.ConfirmAppointment;
using Devclinic.Appointments.Application.Features.RescheduleAppointmentByDoctor;
using Devclinic.Appointments.Application.Features.RescheduleAppointmentByPatient;
using Devclinic.Appointments.Application.Features.ScheduleAppointment;
using Devclinic.Appointments.Domain.SeedWork;

namespace Devclinic.Appointments.Api.Endpoints;

public static class AppointmentsEndpoints
{
    public static RouteGroupBuilder MapAppointmentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/appointments")
            .WithTags("Appointments");

        group.MapPost("/", ScheduleAppointmentAsync)
            .WithName("ScheduleAppointment")
            .Produces<ScheduleAppointmentResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetAppointmentByIdAsync)
            .WithName("GetAppointmentById")
            .Produces<AppointmentDetails>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetAppointmentsByDoctorAsync)
            .WithName("GetAppointmentsByDoctor")
            .Produces<IReadOnlyList<AppointmentSummary>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/confirm", ConfirmAppointmentAsync)
            .WithName("ConfirmAppointment")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/cancel-by-doctor", CancelAppointmentByDoctorAsync)
            .WithName("CancelAppointmentByDoctor")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/cancel-by-patient", CancelAppointmentByPatientAsync)
            .WithName("CancelAppointmentByPatient")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/reschedule-by-doctor", RescheduleAppointmentByDoctorAsync)
            .WithName("RescheduleAppointmentByDoctor")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/reschedule-by-patient", RescheduleAppointmentByPatientAsync)
            .WithName("RescheduleAppointmentByPatient")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> ScheduleAppointmentAsync(
        ScheduleAppointmentRequest request,
        ScheduleAppointmentUseCase useCase,
        CancellationToken ct)
    {
        if (request.DoctorId == Guid.Empty || request.PatientId == Guid.Empty)
            return BadRequest("DoctorId and PatientId are required.");

        return await ExecuteCommandAsync(async () =>
        {
            var id = await useCase.ExecuteAsync(
                new ScheduleAppointmentCommand(request.DoctorId, request.PatientId, request.Time),
                ct);

            return TypedResults.Created(
                $"/appointments/{id.Value}",
                new ScheduleAppointmentResponse(id.Value));
        });
    }

    private static async Task<IResult> GetAppointmentByIdAsync(
        Guid id,
        IAppointmentQueries queries,
        CancellationToken ct)
    {
        var appointment = await queries.GetByIdAsync(id, ct);

        return appointment is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(appointment);
    }

    private static async Task<IResult> GetAppointmentsByDoctorAsync(
        Guid doctorId,
        DateOnly from,
        DateOnly to,
        IAppointmentQueries queries,
        CancellationToken ct)
    {
        if (doctorId == Guid.Empty)
            return BadRequest("DoctorId is required.");

        if (to < from)
            return BadRequest("To must be greater than or equal to from.");

        var appointments = await queries.GetByDoctorAsync(doctorId, from, to, ct);

        return TypedResults.Ok(appointments);
    }

    private static Task<IResult> ConfirmAppointmentAsync(
        Guid id,
        ConfirmAppointmentRequest request,
        ConfirmAppointmentUseCase useCase,
        CancellationToken ct)
    {
        if (request.DoctorId == Guid.Empty)
            return Task.FromResult(BadRequest("DoctorId is required."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(new ConfirmAppointmentCommand(id, request.DoctorId), ct);
            return TypedResults.NoContent();
        });
    }

    private static Task<IResult> CancelAppointmentByDoctorAsync(
        Guid id,
        CancelAppointmentByDoctorRequest request,
        CancelAppointmentByDoctorUseCase useCase,
        CancellationToken ct)
    {
        if (request.DoctorId == Guid.Empty)
            return Task.FromResult(BadRequest("DoctorId is required."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(
                new CancelAppointmentByDoctorCommand(request.DoctorId, id, request.Reason),
                ct);

            return TypedResults.NoContent();
        });
    }

    private static Task<IResult> CancelAppointmentByPatientAsync(
        Guid id,
        CancelAppointmentByPatientRequest request,
        CancelAppointmentByPatientUseCase useCase,
        CancellationToken ct)
    {
        if (request.PatientId == Guid.Empty)
            return Task.FromResult(BadRequest("PatientId is required."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(
                new CancelAppointmentByPatientCommand(request.PatientId, id, request.Reason),
                ct);

            return TypedResults.NoContent();
        });
    }

    private static Task<IResult> RescheduleAppointmentByDoctorAsync(
        Guid id,
        RescheduleAppointmentByDoctorRequest request,
        RescheduleAppointmentByDoctorUseCase useCase,
        CancellationToken ct)
    {
        if (request.DoctorId == Guid.Empty)
            return Task.FromResult(BadRequest("DoctorId is required."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(
                new RescheduleAppointmentByDoctorCommand(id, request.DoctorId, request.NewTime),
                ct);

            return TypedResults.NoContent();
        });
    }

    private static Task<IResult> RescheduleAppointmentByPatientAsync(
        Guid id,
        RescheduleAppointmentByPatientRequest request,
        RescheduleAppointmentByPatientUseCase useCase,
        CancellationToken ct)
    {
        if (request.PatientId == Guid.Empty)
            return Task.FromResult(BadRequest("PatientId is required."));

        return ExecuteCommandAsync(async () =>
        {
            await useCase.ExecuteAsync(
                new RescheduleAppointmentByPatientCommand(id, request.PatientId, request.NewTime),
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
        catch (InvalidOperationException exception) when (IsNotFound(exception))
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status404NotFound,
                title: "Appointment not found");
        }
        catch (InvalidOperationException exception)
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Appointment command rejected");
        }
        catch (DomainException exception)
        {
            return TypedResults.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "Appointment command rejected");
        }
    }

    private static IResult BadRequest(string detail) =>
        TypedResults.Problem(
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid appointment request");

    private static bool IsNotFound(InvalidOperationException exception) =>
        exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
        exception.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
}

public sealed record ScheduleAppointmentRequest(Guid DoctorId, Guid PatientId, DateTime Time);

public sealed record ScheduleAppointmentResponse(Guid Id);

public sealed record ConfirmAppointmentRequest(Guid DoctorId);

public sealed record CancelAppointmentByDoctorRequest(Guid DoctorId, string Reason);

public sealed record CancelAppointmentByPatientRequest(Guid PatientId, string Reason);

public sealed record RescheduleAppointmentByDoctorRequest(Guid DoctorId, DateTime NewTime);

public sealed record RescheduleAppointmentByPatientRequest(Guid PatientId, DateTime NewTime);