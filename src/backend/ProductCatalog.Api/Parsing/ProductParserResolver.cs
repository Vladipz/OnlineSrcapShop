namespace ProductCatalog.Api.Parsing;

public sealed class ProductParserResolver(IEnumerable<IProductParser> parsers)
{
    public IProductParser? Resolve(Uri sourceUri) => parsers.FirstOrDefault(parser => parser.CanParse(sourceUri));
}
