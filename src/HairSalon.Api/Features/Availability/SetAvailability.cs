using ErrorOr;
using ErrorOrAspNetCoreExtensions;
using HairSalon.Api.Constants;
using HairSalon.Api.Data;
using HairSalon.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace HairSalon.Api.Features.Availability;

public static class SetAvailability
{
    public static IEndpointRouteBuilder MapSetAvailability(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (Guid barberId, Request request, IMediator mediator, CancellationToken ct) =>
            {
                var command = new Command(barberId, request.DayOfWeek, request.StartTime, request.EndTime);

                var result = await mediator.Send(command, ct);

                return result.ToOk();
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = AppRoles.Admin })
            .WithName("SetAvailability")
            .WithSummary("Sets availability for the given day of week.")
            .WithDescription("Sets availability for the given day of week.")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<Response>();

        return app;
    }

    public record Command(
        Guid BarberId,
        DayOfWeek DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime) : IRequest<ErrorOr<Response>>;

    public class Handler(AppDbContext db) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var isBarber = await db.UserRoles
                .AnyAsync(ur => ur.UserId == command.BarberId &&
                                db.Roles.Any(r => r.Id == ur.RoleId && r.Name == AppRoles.Barber), ct);

            if (!isBarber)
                return Error.NotFound(
                    "Barber.NotFound",
                    $"Barber with ID '{command.BarberId}' was not found.");

            var existingAvailability = await db.BarberAvailabilities
                .FirstOrDefaultAsync(a => a.BarberId == command.BarberId && a.DayOfWeek == command.DayOfWeek, ct);

            if (existingAvailability is not null)
            {
                var updateResult = existingAvailability.UpdateHours(command.StartTime, command.EndTime);
                if (updateResult.IsError)
                    return updateResult.Errors;

                await db.SaveChangesAsync(ct);

                return new Response(
                    existingAvailability.Id,
                    existingAvailability.BarberId,
                    existingAvailability.DayOfWeek,
                    existingAvailability.StartTime,
                    existingAvailability.EndTime,
                    existingAvailability.CreatedAt,
                    existingAvailability.UpdatedAt);
            }

            var createResult = BarberAvailability.Create(
                command.BarberId,
                command.DayOfWeek,
                command.StartTime,
                command.EndTime);

            if (createResult.IsError)
                return createResult.Errors;

            var newAvailability = createResult.Value;

            db.BarberAvailabilities.Add(newAvailability);
            await db.SaveChangesAsync(ct);

            return new Response(
                newAvailability.Id,
                newAvailability.BarberId,
                newAvailability.DayOfWeek,
                newAvailability.StartTime,
                newAvailability.EndTime,
                newAvailability.CreatedAt,
                newAvailability.UpdatedAt);
        }
    }

    public record Request(
        DayOfWeek DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public record Response(
        Guid Id,
        Guid BarberId,
        DayOfWeek DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);
}