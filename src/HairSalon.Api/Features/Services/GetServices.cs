using HairSalon.Api.Common;
using HairSalon.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HairSalon.Api.Features.Services;

public static class GetServices
{
    public static IEndpointRouteBuilder MapGetServices(this IEndpointRouteBuilder app)
    {
        app.MapGet("/",
                async (IMediator mediator, CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int size = 10) =>
                {
                    var query = new Query(page, size);
                
                    return await mediator.Send(query, ct);
                })
            .RequireAuthorization()
            .WithName("GetServices")
            .WithSummary("Get salon services")
            .WithDescription("Get salon services")
            .Produces<PagedResult<ServiceDto>>(); 

        return app;
    }
    
    public record Query(int PageNumber, int PageSize) : IRequest<PagedResult<ServiceDto>>;

    public record ServiceDto(
        Guid Id,
        string Name,
        int DurationInMinutes);
    
    public class Handler(AppDbContext db) : IRequestHandler<Query, PagedResult<ServiceDto>>
    {
        public async Task<PagedResult<ServiceDto>> Handle(Query query, CancellationToken ct)
        {
            var services = await db.Services
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.Id)
                .Select(s => new ServiceDto(s.Id, s.Name, (int)s.Duration.TotalMinutes))
                .ToPagedResultAsync(query.PageNumber, query.PageSize, ct);

            return services;
        }
    }
}