namespace HairSalon.Api.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserEmail { get; }
    bool IsAuthenticated { get; }
    Guid GetUserId();
    bool IsInRole(string role);
}