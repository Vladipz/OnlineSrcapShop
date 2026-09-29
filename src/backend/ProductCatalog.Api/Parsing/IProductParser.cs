namespace ProductCatalog.Api.Parsing;

public interface IProductParser
{
    bool CanParse(Uri sourceUri);
    Task<ParseResult> ParseAsync(Uri sourceUri, CancellationToken cancellationToken = default);
}
