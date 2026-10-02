using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using ProductCatalog.Api.Auth;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Imports;
using ProductCatalog.Api.Parsing;
using ProductCatalog.Api.Products;

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

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "ProductCatalog.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.CatalogRead, policy =>
                policy.RequireAuthenticatedUser().RequireRole(Roles.Admin, Roles.User))
            .AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                policy.RequireAuthenticatedUser().RequireRole(Roles.Admin));

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

    public static IServiceCollection AddCatalogFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<ProductImportService>();
        services.AddTransient<SourcePageClient>();
        services.AddTransient<IProductParser, BooksToScrapeParser>();
        services.AddOptions<ParserOptions>().Bind(configuration.GetSection(ParserOptions.SectionName))
            .Validate(options => options.ProductLimit is >= 1 and <= 20, "ProductLimit must be between 1 and 20.")
            .Validate(options => options.MaxConcurrency is >= 1 and <= 4, "MaxConcurrency must be between 1 and 4.")
            .Validate(options => options.RequestTimeoutSeconds is >= 1 and <= 60,
                "RequestTimeoutSeconds must be between 1 and 60.")
            .ValidateOnStart();
        services.AddHttpClient(SourcePageClient.ClientName, client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ProductCatalog/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html");
        }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            UseProxy = false,
            ConnectCallback = SourceUrlPolicy.ConnectAsync,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 4
        });

        return services;
    }

    private static bool IsValidAccount(SeedAccountOptions account) =>
        !string.IsNullOrWhiteSpace(account.Email)
        && account.Email == account.Email.Trim()
        && new EmailAddressAttribute().IsValid(account.Email)
        && !string.IsNullOrWhiteSpace(account.Password);
}
