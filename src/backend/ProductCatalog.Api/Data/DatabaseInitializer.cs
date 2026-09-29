using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using ProductCatalog.Api.Auth;

namespace ProductCatalog.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var connection = new SqliteConnectionStringBuilder(context.Database.GetConnectionString());
        var directory = Path.GetDirectoryName(connection.DataSource)
            ?? throw new InvalidOperationException("The SQLite database must have a parent directory.");

        Directory.CreateDirectory(directory);

        await context.Database.MigrateAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync(cancellationToken);

        app.Logger.LogInformation("Database migrations and Identity initialization completed.");
    }
}
