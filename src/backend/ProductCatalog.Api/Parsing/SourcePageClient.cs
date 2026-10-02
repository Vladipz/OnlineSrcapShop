using System.Net;
using System.Text;

using Microsoft.Extensions.Options;

namespace ProductCatalog.Api.Parsing;

public sealed class SourcePageClient(IHttpClientFactory clientFactory, IOptions<ParserOptions> options)
{
    public const string ClientName = "ProductSource";
    private const int MaxPageBytes = 2 * 1024 * 1024;

    public async Task<(string Html, Uri FinalUri)> LoadAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.RequestTimeoutSeconds));
        using var client = clientFactory.CreateClient(ClientName);
        try
        {
            for (var redirects = 0; redirects <= 3; redirects++)
            {
                if (!SourceUrlPolicy.IsSupported(uri))
                {
                    throw new SourceParseException("A source link or redirect targeted an unsupported URL.");
                }

                using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect
                    or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
                {
                    var location = response.Headers.Location
                        ?? throw new SourceParseException("The source redirect has no location.");
                    uri = SourceUrlPolicy.Normalize(new Uri(uri, location));
                    continue;
                }

                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentType?.MediaType is not ("text/html" or "application/xhtml+xml")
                    || response.Content.Headers.ContentLength > MaxPageBytes)
                {
                    throw new SourceParseException("The source response is not a supported HTML document.");
                }

                await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
                using var buffer = new MemoryStream();
                var bytes = new byte[8192];
                int count;
                while ((count = await stream.ReadAsync(bytes, timeout.Token)) > 0)
                {
                    if (buffer.Length + count > MaxPageBytes)
                    {
                        throw new SourceParseException("The source document is too large.");
                    }

                    buffer.Write(bytes, 0, count);
                }

                // Books to Scrape declares UTF-8. Never load embedded resources.
                return (Encoding.UTF8.GetString(buffer.ToArray()), uri);
            }

            throw new SourceParseException("The source exceeded the redirect limit.");
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new SourceParseException("The source request timed out.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new SourceParseException("The source request failed.", exception);
        }
        catch (IOException exception)
        {
            throw new SourceParseException("The source response could not be read.", exception);
        }
        catch (UriFormatException exception)
        {
            throw new SourceParseException("The source returned an invalid redirect URL.", exception);
        }
    }
}
