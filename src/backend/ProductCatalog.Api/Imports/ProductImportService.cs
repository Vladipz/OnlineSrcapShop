using Microsoft.EntityFrameworkCore;

using ProductCatalog.Api.Data;
using ProductCatalog.Api.Parsing;
using ProductCatalog.Api.Products;

namespace ProductCatalog.Api.Imports;

public sealed class ProductImportService(AppDbContext db, ILogger<ProductImportService> logger)
{
    public async Task<ImportPreviewResponse> PreviewAsync(Uri uri, IProductParser parser, CancellationToken ct)
    {
        var parsed = await ParseValidatedAsync(uri, parser, ct);
        var seen = await GetExistingAsync(parsed, ct);
        var products = parsed.Products.Select(product => new PreviewProduct(
            product.Name, product.Description, product.ImageUrl, product.Price, product.CurrencyCode,
            product.SourceUrl, !seen.Add(product.SourceUrl))).ToArray();
        var duplicates = products.Count(product => product.AlreadyImported);
        logger.LogInformation("Import preview: {Found} found, {New} new, {Duplicates} duplicates, {Failed} failed",
            parsed.Found, products.Length - duplicates, duplicates, parsed.Failed);
        return new ImportPreviewResponse(uri.AbsoluteUri, parsed.Found, products.Length - duplicates,
            duplicates, parsed.Failed, products);
    }

    public async Task<ImportConfirmResponse> ConfirmAsync(Uri uri, IProductParser parser, CancellationToken ct)
    {
        var parsed = await ParseValidatedAsync(uri, parser, ct);
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
        logger.LogInformation("Import confirmed: {Found} found, {Created} created, {Skipped} skipped, {Failed} failed",
            parsed.Found, created, skipped, parsed.Failed);
        return new ImportConfirmResponse(parsed.Found, created, skipped, parsed.Failed);
    }

    private async Task<ParseResult> ParseValidatedAsync(Uri uri, IProductParser parser, CancellationToken ct)
    {
        var parsed = await parser.ParseAsync(uri, ct);
        var valid = parsed.Products.Where(product =>
            ProductValidation.Validate(new UpdateProductRequest(product.Name, product.Description,
                product.ImageUrl, product.Price, product.CurrencyCode)).Count == 0
            && ProductValidation.IsHttpUrl(product.SourceUrl)).ToArray();
        var invalid = parsed.Products.Count - valid.Length;
        if (invalid > 0)
        {
            logger.LogWarning("Skipped {InvalidCount} parsed products with invalid data", invalid);
        }

        return new ParseResult(parsed.Found, parsed.Failed + invalid, valid);
    }

    private async Task<HashSet<string>> GetExistingAsync(ParseResult parsed, CancellationToken ct)
    {
        var sourceUrls = parsed.Products.Select(product => product.SourceUrl).Distinct().ToArray();
        return (await db.Products.AsNoTracking().Where(product => sourceUrls.Contains(product.SourceUrl))
            .Select(product => product.SourceUrl).ToListAsync(ct)).ToHashSet(StringComparer.Ordinal);
    }
}
