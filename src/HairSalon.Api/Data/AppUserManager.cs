using HairSalon.Api.Constants;
using HairSalon.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HairSalon.Api.Data;

public class AppUserManager(
    IUserStore<AppUser> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<AppUser> passwordHasher,
    IEnumerable<IUserValidator<AppUser>> userValidators,
    IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<AppUser>> logger)
    : UserManager<AppUser>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators,
        keyNormalizer, errors, services, logger)
{
    public override async Task<IdentityResult> CreateAsync(AppUser user, string password)
    {
        var result = await base.CreateAsync(user, password);

        if (result.Succeeded)
            await AddToRoleAsync(user, AppRoles.Client);

        return result;
    }
}
