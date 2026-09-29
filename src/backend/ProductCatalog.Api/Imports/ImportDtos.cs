namespace ProductCatalog.Api.Imports;

public sealed record ImportRequest(string? SourceUrl);

public sealed record PreviewProduct(
    string Name, string? Description, string ImageUrl, decimal? Price,
    string? CurrencyCode, string SourceUrl, bool AlreadyImported);

public sealed record ImportPreviewResponse(
    string SourceUrl, int Found, int NewCount, int DuplicateCount, int Failed, IReadOnlyList<PreviewProduct> Products);

public sealed record ImportConfirmResponse(int Found, int Created, int Skipped, int Failed);
