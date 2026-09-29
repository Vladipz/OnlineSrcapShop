namespace ProductCatalog.Api.Parsing;

public sealed record ParsedProduct(
    string Name, string? Description, string ImageUrl, decimal? Price, string? CurrencyCode, string SourceUrl);

public sealed record ParseResult(int Found, int Failed, IReadOnlyList<ParsedProduct> Products);
