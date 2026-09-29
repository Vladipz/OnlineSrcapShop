using System.Net;
using System.Net.Sockets;

namespace ProductCatalog.Api.Parsing;

public static class SourceUrlPolicy
{
    public const string SupportedHost = "books.toscrape.com";

    public static bool IsSupported(Uri uri) =>
        uri.IsAbsoluteUri && uri.AbsoluteUri.Length <= 2048 && uri.Scheme is "http" or "https"
        && string.Equals(uri.IdnHost, SupportedHost, StringComparison.OrdinalIgnoreCase)
        && uri.IsDefaultPort && string.IsNullOrEmpty(uri.UserInfo);

    public static Uri Normalize(Uri uri) => new UriBuilder(uri) { Fragment = "" }.Uri;

    public static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return bytes[0] is not (0 or 10 or 127) && bytes[0] < 224
                && !(bytes[0] == 169 && bytes[1] == 254)
                && !(bytes[0] == 172 && bytes[1] is >= 16 and <= 31)
                && !(bytes[0] == 192 && bytes[1] == 168)
                && !(bytes[0] == 100 && bytes[1] is >= 64 and <= 127)
                && !(bytes[0] == 192 && bytes[1] == 0)
                && !(bytes[0] == 198 && bytes[1] is 18 or 19)
                && !(bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100)
                && !(bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113);
        }

        // Only global unicast IPv6, excluding documentation ranges.
        return address.AddressFamily == AddressFamily.InterNetworkV6
            && (bytes[0] & 0xe0) == 0x20
            && !(bytes[0] == 0x20 && bytes[1] == 0x02)
            && !(bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] < 2)
            && !(bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0d && bytes[3] == 0xb8);
    }

    public static async ValueTask<Stream> ConnectAsync(
        SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        if (!string.Equals(context.DnsEndPoint.Host, SupportedHost, StringComparison.OrdinalIgnoreCase)
            || context.DnsEndPoint.Port is not (80 or 443))
        {
            throw new HttpRequestException("Unsupported connection target.");
        }

        var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
        if (addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
        {
            throw new HttpRequestException("The source resolved to a disallowed address.");
        }

        // Connect to the validated IP itself, avoiding a second DNS lookup/rebinding.
        foreach (var address in addresses)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (SocketException)
            {
                socket.Dispose();
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        throw new HttpRequestException("Unable to connect to the supported source.");
    }
}
