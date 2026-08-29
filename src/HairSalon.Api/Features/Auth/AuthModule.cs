using HairSalon.Api.Data.Entities;

namespace HairSalon.Api.Features.Auth;

public static class AuthModule
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/auth")
            .WithTags("Authentication");

        authGroup.MapIdentityApi<AppUser>();

        return app;
    }
}
