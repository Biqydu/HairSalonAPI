using FluentValidation;
using HairSalon.Api.Constants;
using HairSalon.Api.Data;
using HairSalon.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace HairSalon.Api.Features.Services;

public static class CreateService
{
    public static IEndpointRouteBuilder MapCreateService(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (Request request, IMediator mediator, CancellationToken ct) =>
            {
                var command = new Command(request.Name, request.Description, request.Price, request.Duration, request.IsActive);
                
                var result  = await mediator.Send(command, ct);

                return Results.Created($"api/services/{result.ServiceId}", result);
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = AppRoles.Admin })
            .WithName("CreateService")
            .WithSummary("Creates a new service")
            .WithDescription("Creates a new salon service");

        return app;
    }

    public record Request(string Name, string? Description, decimal Price, int Duration, bool IsActive);

    public record Command(string Name, string? Description, decimal Price, int Duration, bool IsActive)
        : IRequest<Response>;

    public class Handler(AppDbContext db) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command command, CancellationToken ct)
        {
            var service = new Service
            {
                Name = command.Name,
                Price = command.Price,
                Duration = TimeSpan.FromMinutes(command.Duration),
                IsActive = command.IsActive
            };
            
            db.Services.Add(service);
            await db.SaveChangesAsync(ct);

            return new Response(service.Id);
        }
    }

    public record Response(Guid ServiceId);
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            const int minDuration = 5;
            const int maxDuration = 360;

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .MinimumLength(3)
                .WithMessage("Name must have at least 3 characters")
                .MaximumLength(50)
                .WithMessage("Name cannot exceed 50 characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .MinimumLength(5)
                .WithMessage("Description must have at least 5 characters")
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .When(c => !string.IsNullOrEmpty(c.Description));

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0");

            RuleFor(x => x.Duration)
                .InclusiveBetween(minDuration, maxDuration)
                .WithMessage(
                    $"Duration must be between {minDuration} and {maxDuration} minutes.")
                .Must(duration => duration % 5 == 0)
                .WithMessage("Duration must be a multiple of 5 minutes.");
        }
    }
}