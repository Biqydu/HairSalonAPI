using ErrorOr;
using ErrorOrAspNetCoreExtensions;
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
                var command = new Command(
                    request.Name, 
                    request.Description, 
                    request.Price, 
                    request.Duration, 
                    request.IsActive);
                
                var result = await mediator.Send(command, ct);

                return result.ToCreated(response => $"api/services/{response.ServiceId}");
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = AppRoles.Admin })
            .WithName("CreateService")
            .WithSummary("Creates a new service")
            .WithDescription("Creates a new salon service")
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    public record Request(string Name, string? Description, decimal Price, int Duration, bool IsActive);

    public record Command(string Name, string? Description, decimal Price, int Duration, bool IsActive)
        : IRequest<ErrorOr<Response>>;

    public class Handler(AppDbContext db) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var serviceResult = Service.Create(
                command.Name,
                command.Description,
                command.Price,
                TimeSpan.FromMinutes(command.Duration),
                command.IsActive);

            if (serviceResult.IsError)
                return serviceResult.Errors;
            
            var service = serviceResult.Value;
            
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
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");

            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than 0.");
        }
    }
}