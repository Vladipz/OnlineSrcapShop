namespace ProductCatalog.Api.Products;

public sealed class Product
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public required string ImageUrl { get; set; }

    public decimal? Price { get; set; }

    public string? CurrencyCode { get; set; }

    public required string SourceUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
