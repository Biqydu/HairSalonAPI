using ErrorOr;
using ErrorOrAspNetCoreExtensions;
using FluentValidation;
using HairSalon.Api.Constants;
using HairSalon.Api.Data;
using HairSalon.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace HairSalon.Api.Features.Availability;

public static class SetWeeklyAvailability
{
    public static IEndpointRouteBuilder MapSetWeeklyAvailability(this IEndpointRouteBuilder app)
    {
        app.MapPut("/", async (
                Guid barberId,
                Request request,
                ISender mediator,
                CancellationToken ct) =>
            {
                var command = new Command(barberId, request.Days);
                var result = await mediator.Send(command, ct);

                return result.ToOk();
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = AppRoles.Admin })
            .WithName("SetWeeklyAvailability")
            .WithSummary("Sets or replaces the entire weekly availability schedule for a barber.")
            .WithDescription("Sets or replaces the entire weekly availability schedule for a barber.")
            .Produces<Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    public record Request(List<DayAvailabilityDto> Days);

    public record DayAvailabilityDto(
        DayOfWeek DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public record Command(Guid BarberId, List<DayAvailabilityDto> Days) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Days)
                .NotNull().WithMessage("Days is required");

            RuleFor(x => x.Days)
                .NotNull()
                .WithMessage("Days list cannot be null.")
                .NotEmpty()
                .WithMessage("At least one day availability must be provided.");
        }
    }

    public record Response(List<AvailabilityItemDto> Availabilities);

    public record AvailabilityItemDto(
        Guid Id,
        DayOfWeek DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime);

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

            var existingAvailabilities = await db.BarberAvailabilities
                .Where(a => a.BarberId == command.BarberId)
                .ToListAsync(ct);

            var incomingDays = command.Days.Select(d => d.DayOfWeek).ToHashSet();
            var toRemove = existingAvailabilities.Where(a => !incomingDays.Contains(a.DayOfWeek)).ToList();

            if (toRemove.Count > 0)
                db.BarberAvailabilities.RemoveRange(toRemove);

            var updatedOrCreatedList = new List<BarberAvailability>();

            foreach (var day in command.Days)
            {
                var existing = existingAvailabilities.FirstOrDefault(a => a.DayOfWeek == day.DayOfWeek);

                if (existing is not null)
                {
                    var updateResult = existing.UpdateHours(day.StartTime, day.EndTime);
                    if (updateResult.IsError)
                        return updateResult.Errors;

                    updatedOrCreatedList.Add(existing);
                }
                else
                {
                    var createResult = BarberAvailability.Create(
                        command.BarberId,
                        day.DayOfWeek,
                        day.StartTime,
                        day.EndTime);

                    if (createResult.IsError)
                        return createResult.Errors;

                    var newEntity = createResult.Value;
                    db.BarberAvailabilities.Add(newEntity);
                    updatedOrCreatedList.Add(newEntity);
                }
            }

            await db.SaveChangesAsync(ct);

            var responseItems = updatedOrCreatedList
                .OrderBy(a => a.DayOfWeek)
                .Select(a => new AvailabilityItemDto(a.Id, a.DayOfWeek, a.StartTime, a.EndTime))
                .ToList();

            return new Response(responseItems);
        }
    }
}