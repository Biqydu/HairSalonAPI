using Microsoft.AspNetCore.Identity;

namespace HairSalon.Api.Data.Entities;

public sealed class AppRole : IdentityRole<Guid>
{
    public AppRole()
    {
        Id = Guid.CreateVersion7();
    }
    
    public AppRole(string roleName) : base(roleName)
    {
        Id = Guid.CreateVersion7();
    }
}