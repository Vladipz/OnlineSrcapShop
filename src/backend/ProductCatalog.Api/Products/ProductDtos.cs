namespace ProductCatalog.Api.Products;

public sealed record UpdateProductRequest(
    string? Name, string? Description, string? ImageUrl, decimal? Price, string? CurrencyCode);

public sealed record ProductResponse(
    int Id, string Name, string? Description, string ImageUrl, decimal? Price,
    string? CurrencyCode, string SourceUrl, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
