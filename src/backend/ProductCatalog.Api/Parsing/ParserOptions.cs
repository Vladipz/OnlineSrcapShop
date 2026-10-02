namespace ProductCatalog.Api.Parsing;

public sealed class ParserOptions
{
    public const string SectionName = "Parser";
    public int ProductLimit { get; init; } = 20;
    public int RequestTimeoutSeconds { get; init; } = 15;
    public int MaxConcurrency { get; init; } = 4;
}
