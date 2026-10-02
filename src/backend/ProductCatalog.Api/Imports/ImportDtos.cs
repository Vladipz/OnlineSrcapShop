namespace ProductCatalog.Api.Imports;

public sealed record ImportRequest(string? SourceUrl);

public sealed record ImportResponse(int Found, int Created, int Skipped, int Failed);
