using ErrorOr;
using ErrorOrAspNetCoreExtensions;
using HairSalon.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HairSalon.Api.Features.Services;

public static class GetServiceDetailsById
{
    public static IEndpointRouteBuilder MapGetServiceDetailsById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{serviceId:guid}", async (Guid serviceId, IMediator mediator, CancellationToken ct) =>
            {
                var query = new Query(serviceId);

                var result = await mediator.Send(query, ct);

                return result.ToOk();
            })
            .RequireAuthorization()
            .WithName("GetServiceDetailsById")
            .WithSummary("Gets a service details by id")
            .WithDescription("Gets a service details by id")
            .Produces<Response>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    public record Query(Guid ServiceId) : IRequest<ErrorOr<Response>>;

    public record Response(
        Guid Id,
        string Name,
        string? Description,
        int DurationInMinutes
    );

    public class Handler(AppDbContext db)
        : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken ct)
        {
            var service = await db.Services
                .AsNoTracking()
                .Where(s => s.Id == query.ServiceId && s.IsActive)
                .FirstOrDefaultAsync(ct);

            if (service is null)
                return Error.NotFound("Service.NotFound", "The service was not found.");

            return new Response(
                service.Id,
                service.Name,
                service.Description,
                (int)service.Duration.TotalMinutes
            );
        }
    }
}