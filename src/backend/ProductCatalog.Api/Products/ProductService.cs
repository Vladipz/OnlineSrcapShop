using Microsoft.EntityFrameworkCore;

using ProductCatalog.Api.Data;

namespace ProductCatalog.Api.Products;

public sealed class ProductService(AppDbContext db, ILogger<ProductService> logger)
{
    public async Task<ProductResponse[]> GetAllAsync(CancellationToken cancellationToken) =>
        (await db.Products.AsNoTracking().OrderBy(product => product.Id)
            .ToListAsync(cancellationToken)).Select(product => product.ToResponse()).ToArray();

    public async Task<ProductResponse?> GetAsync(int id, CancellationToken cancellationToken) =>
        (await db.Products.AsNoTracking().SingleOrDefaultAsync(product => product.Id == id, cancellationToken))
            ?.ToResponse();

    public async Task<ProductResponse?> UpdateAsync(
        int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Name = request.Name!.Trim();
        product.Description = request.Description;
        product.ImageUrl = request.ImageUrl!;
        product.Price = request.Price;
        product.CurrencyCode = request.CurrencyCode;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated product {ProductId}", id);
        return product.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var deleted = await db.Products.Where(product => product.Id == id).ExecuteDeleteAsync(cancellationToken);
        if (deleted > 0)
        {
            logger.LogInformation("Deleted product {ProductId}", id);
        }

        return deleted > 0;
    }
}
