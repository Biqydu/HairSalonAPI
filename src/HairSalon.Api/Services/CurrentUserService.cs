using System.Security.Claims;

namespace HairSalon.Api.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var userIdStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdStr, out var userId)
                ? userId
                : null;
        }
    }

    public string? UserEmail =>
        User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public Guid GetUserId()
    {
        return UserId
               ?? throw new UnauthorizedAccessException(
                   "User is not authenticated or claims are invalid.");
    }

    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) ?? false;
    }
}