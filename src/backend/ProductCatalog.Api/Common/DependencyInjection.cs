using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using ProductCatalog.Api.Auth;
using ProductCatalog.Api.Data;

namespace ProductCatalog.Api.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var configuredConnection = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(configuredConnection))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
        }

        var connection = new SqliteConnectionStringBuilder(configuredConnection);

        if (string.IsNullOrWhiteSpace(connection.DataSource)
            || connection.DataSource == ":memory:"
            || connection.Mode == SqliteOpenMode.Memory
            || connection.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Configure a filesystem path for the persistent SQLite database.");
        }

        connection.DataSource = Path.GetFullPath(connection.DataSource, environment.ContentRootPath);

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection.ConnectionString));

        return services;
    }

    public static IServiceCollection AddApplicationIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddOptions<SeedUsersOptions>()
            .Bind(configuration.GetSection(SeedUsersOptions.SectionName))
            .Validate(options => !options.Enabled || IsValidAccount(options.Admin),
                "Enabled account seeding requires a valid Admin email and a nonempty password.")
            .Validate(options => !options.Enabled || IsValidAccount(options.User),
                "Enabled account seeding requires a valid User email and a nonempty password.")
            .Validate(options => !options.Enabled
                || !string.Equals(options.Admin.Email, options.User.Email, StringComparison.OrdinalIgnoreCase),
                "Admin and User seed emails must be different.")
            .ValidateOnStart();

        services.AddScoped<IdentitySeeder>();

        return services;
    }

    private static bool IsValidAccount(SeedAccountOptions account) =>
        !string.IsNullOrWhiteSpace(account.Email)
        && account.Email == account.Email.Trim()
        && new EmailAddressAttribute().IsValid(account.Email)
        && !string.IsNullOrWhiteSpace(account.Password);
}
