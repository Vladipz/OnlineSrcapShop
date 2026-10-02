using Microsoft.EntityFrameworkCore;

using ProductCatalog.Api.Data;
using ProductCatalog.Api.Parsing;
using ProductCatalog.Api.Products;

namespace ProductCatalog.Api.Imports;

public sealed class ProductImportService(
    AppDbContext db, IProductParser parser, ILogger<ProductImportService> logger)
{
    public async Task<ImportResponse> ImportAsync(Uri uri, CancellationToken ct)
    {
        var parsed = await parser.ParseAsync(uri, ct);
        var seen = await GetExistingAsync(parsed, ct);
        var created = 0;
        var skipped = 0;
        var now = DateTime.UtcNow;
        foreach (var product in parsed.Products)
        {
            if (!seen.Add(product.SourceUrl))
            {
                skipped++;
                continue;
            }

            db.Products.Add(new Product
            {
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                CurrencyCode = product.CurrencyCode,
                SourceUrl = product.SourceUrl,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
            created++;
        }

        // One atomic save. The unique index remains the final guard for concurrent imports.
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Import complete: {Found} found, {Created} created, {Skipped} skipped, {Failed} failed",
            parsed.Found, created, skipped, parsed.Failed);
        return new ImportResponse(parsed.Found, created, skipped, parsed.Failed);
    }

    private async Task<HashSet<string>> GetExistingAsync(ParseResult parsed, CancellationToken ct)
    {
        var sourceUrls = parsed.Products.Select(product => product.SourceUrl).Distinct().ToArray();
        return (await db.Products.AsNoTracking().Where(product => sourceUrls.Contains(product.SourceUrl))
            .Select(product => product.SourceUrl).ToListAsync(ct)).ToHashSet(StringComparer.Ordinal);
    }
}
