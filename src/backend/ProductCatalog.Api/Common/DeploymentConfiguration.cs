using System.Net;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

namespace ProductCatalog.Api.Common;

public static class DeploymentConfiguration
{
    public static IServiceCollection AddDeploymentConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var proxy = configuration["Deployment:KnownProxy"];
        IPAddress? proxyAddress = null;

        if (!string.IsNullOrWhiteSpace(proxy) && !IPAddress.TryParse(proxy, out proxyAddress))
        {
            throw new InvalidOperationException("Deployment:KnownProxy must be an explicit proxy IP address.");
        }

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            // With no configured proxy, forwarded headers are ignored, not trusted globally.
            options.ForwardedHeaders = proxyAddress is null
                ? ForwardedHeaders.None
                : ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;

            if (proxyAddress is not null)
            {
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
                options.KnownProxies.Add(proxyAddress);
            }
        });

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
