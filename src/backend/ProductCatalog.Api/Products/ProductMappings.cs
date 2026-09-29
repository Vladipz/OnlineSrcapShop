namespace ProductCatalog.Api.Products;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id, product.Name, product.Description, product.ImageUrl, product.Price,
        product.CurrencyCode, product.SourceUrl,
        DateTime.SpecifyKind(product.CreatedAtUtc, DateTimeKind.Utc),
        DateTime.SpecifyKind(product.UpdatedAtUtc, DateTimeKind.Utc));
}
