namespace HairSalon.Api.Features.Services;

public static class ServicesModule
{
    public static IEndpointRouteBuilder MapServicesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/services")
            .WithTags("Salon Services");

        group
            .MapCreateService()
            .MapGetServiceDetailsById();

        return app;
    }
}