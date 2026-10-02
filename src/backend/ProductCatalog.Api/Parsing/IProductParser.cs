namespace ProductCatalog.Api.Parsing;

public interface IProductParser
{
    Task<ParseResult> ParseAsync(Uri sourceUri, CancellationToken cancellationToken = default);
}
