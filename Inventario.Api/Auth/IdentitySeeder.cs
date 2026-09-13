using Microsoft.AspNetCore.Identity;

namespace Inventario.Api.Auth;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Could not create role '{role}'.");
                }
            }
        }
    }

    public static async Task SeedAdministratorAsync(IServiceProvider services, IConfiguration configuration)
    {
        var options = configuration.GetRequiredSection(BootstrapAdminOptions.SectionName).Get<BootstrapAdminOptions>();
        if (options is null || string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            return;
        }

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await users.FindByEmailAsync(options.Email) is not null)
        {
            return;
        }

        var user = new ApplicationUser { UserName = options.Email, Email = options.Email, DisplayName = options.DisplayName };
        var result = await users.CreateAsync(user, options.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Could not create the bootstrap administrator.");
        }

        await users.AddToRoleAsync(user, ApplicationRoles.Administrator);
    }
}
