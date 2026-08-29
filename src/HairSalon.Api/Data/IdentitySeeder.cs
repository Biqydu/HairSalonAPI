using HairSalon.Api.Constants;
using HairSalon.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace HairSalon.Api.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();

        foreach (var roleName in AppRoles.All)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            
            if (!roleExist)
            {
                await roleManager.CreateAsync(new AppRole(roleName));
            }
        }
    }
}