using System.Globalization;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using Microsoft.Extensions.Options;

using ProductCatalog.Api.Products;

namespace ProductCatalog.Api.Parsing;

public sealed class BooksToScrapeParser(
    SourcePageClient pages, IOptions<ParserOptions> options, ILogger<BooksToScrapeParser> logger) : IProductParser
{
    public bool CanParse(Uri sourceUri) => SourceUrlPolicy.IsSupported(sourceUri);

    public async Task<ParseResult> ParseAsync(Uri sourceUri, CancellationToken cancellationToken = default)
    {
        var (html, finalUri) = await pages.LoadAsync(sourceUri, cancellationToken);
        using var document = await new HtmlParser().ParseDocumentAsync(html, cancellationToken);
        var cards = document.QuerySelectorAll("article.product_pod").Take(options.Value.ProductLimit).ToArray();
        if (cards.Length == 0)
        {
            throw new SourceParseException("No recognizable product cards were found.");
        }

        var products = new ParsedProduct?[cards.Length];
        await Parallel.ForEachAsync(Enumerable.Range(0, cards.Length), new ParallelOptions
        {
            MaxDegreeOfParallelism = options.Value.MaxConcurrency,
            CancellationToken = cancellationToken
        }, async (index, ct) =>
        {
            try
            {
                products[index] = await ParseCardAsync(cards[index], finalUri, ct);
            }
            catch (Exception exception) when (exception is SourceParseException or FormatException or UriFormatException)
            {
                logger.LogWarning(exception, "Skipped source product card {CardNumber}", index + 1);
            }
        });

        var successful = products.OfType<ParsedProduct>().ToArray();
        return new ParseResult(cards.Length, cards.Length - successful.Length, successful);
    }

    private async Task<ParsedProduct> ParseCardAsync(IElement card, Uri listingUri, CancellationToken ct)
    {
        var link = card.QuerySelector("h3 a") ?? throw new SourceParseException("Missing product link.");
        var name = (link.GetAttribute("title") ?? link.TextContent).Trim();
        var detailUri = Resolve(listingUri, link.GetAttribute("href"));
        if (!SourceUrlPolicy.IsSupported(detailUri))
        {
            throw new SourceParseException("Unsupported product detail link.");
        }

        var imageUri = Resolve(listingUri, card.QuerySelector("img")?.GetAttribute("src"));
        var priceText = card.QuerySelector(".price_color")?.TextContent.Trim();
        if (priceText is null || !priceText.StartsWith('£')
            || !decimal.TryParse(priceText[1..], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price))
        {
            throw new SourceParseException("Invalid GBP price.");
        }

        var (html, finalUri) = await pages.LoadAsync(detailUri, ct);
        using var document = await new HtmlParser().ParseDocumentAsync(html, ct);
        if (document.QuerySelector("article.product_page h1") is null)
        {
            throw new SourceParseException("Unrecognized product detail page.");
        }

        var description = document.QuerySelector("#product_description + p")?.TextContent.Trim();
        var product = new ParsedProduct(name, description, imageUri.AbsoluteUri, price, "GBP", finalUri.AbsoluteUri);
        if (ProductValidation.Validate(new UpdateProductRequest(
            product.Name, product.Description, product.ImageUrl, product.Price, product.CurrencyCode)).Count > 0
            || !ProductValidation.IsHttpUrl(product.SourceUrl))
        {
            throw new SourceParseException("The parsed product failed validation.");
        }

        return product;
    }

    private static Uri Resolve(Uri baseUri, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(baseUri, value, out var uri))
        {
            throw new SourceParseException("Missing or invalid product URL.");
        }

        return SourceUrlPolicy.Normalize(uri);
    }
}
