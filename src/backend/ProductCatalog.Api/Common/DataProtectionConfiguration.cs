using Microsoft.AspNetCore.DataProtection;

namespace ProductCatalog.Api.Common;

public static class DataProtectionConfiguration
{
    public static IServiceCollection AddDataProtectionConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var keysPath = configuration["Deployment:DataProtectionKeysPath"];

        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            var directory = new DirectoryInfo(Path.GetFullPath(keysPath, environment.ContentRootPath));
            directory.Create();
            services.AddDataProtection()
                .SetApplicationName("ProductCatalog")
                .PersistKeysToFileSystem(directory);
        }

        return services;
    }
}
