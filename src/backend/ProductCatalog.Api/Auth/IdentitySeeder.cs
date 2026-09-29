using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ProductCatalog.Api.Auth;

public sealed class IdentitySeeder(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IOptions<SeedUsersOptions> options,
    ILogger<IdentitySeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRoleAsync(Roles.Admin, cancellationToken);
        await EnsureRoleAsync(Roles.User, cancellationToken);

        if (!options.Value.Enabled)
        {
            logger.LogInformation("Account seeding is disabled; application roles are available.");
            return;
        }

        await EnsureAccountAsync(options.Value.Admin, Roles.Admin, cancellationToken);
        await EnsureAccountAsync(options.Value.User, Roles.User, cancellationToken);
    }

    private async Task EnsureRoleAsync(string role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!await roleManager.RoleExistsAsync(role))
        {
            EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole(role)), "create role");
            logger.LogInformation("Created application role {Role}.", role);
        }
    }

    private async Task EnsureAccountAsync(
        SeedAccountOptions account,
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(account.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = account.Email,
                Email = account.Email
            };

            EnsureSucceeded(await userManager.CreateAsync(user, account.Password), "create seed account");
            logger.LogInformation("Created configured seed account for role {Role}.", role);
        }

        // Existing users keep their password, security stamp, and other account data.
        if (!await userManager.IsInRoleAsync(user, role))
        {
            EnsureSucceeded(await userManager.AddToRoleAsync(user, role), "assign seed account role");
            logger.LogInformation("Assigned configured seed account to role {Role}.", role);
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            var errorCodes = string.Join(", ", result.Errors.Select(error => error.Code));
            throw new InvalidOperationException($"Unable to {operation}. Identity error codes: {errorCodes}.");
        }
    }
}
