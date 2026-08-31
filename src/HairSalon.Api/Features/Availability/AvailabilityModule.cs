namespace HairSalon.Api.Features.Availability;

public static class AvailabilityModule
{
    public static IEndpointRouteBuilder MapAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("barbers/{barberId:guid}/availabilities")
            .WithTags("Barber Availability");
        
        return app;
    }
}