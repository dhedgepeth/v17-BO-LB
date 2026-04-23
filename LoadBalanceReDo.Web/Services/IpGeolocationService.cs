using System.Net;
using LoadBalanceReDo.Web.Models;

namespace LoadBalanceReDo.Web.Services;

public class IpGeolocationService : IIpGeolocationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IpGeolocationService> _logger;

    public IpGeolocationService(IHttpClientFactory httpClientFactory, ILogger<IpGeolocationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IpLocation?> GetLocationAsync(
        string? ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return null;

        // Skip loopback and private IPs — ip-api will fail on them anyway
        if (IPAddress.TryParse(ipAddress, out var parsed) && IsLocalOrPrivate(parsed))
            return null;

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);

            var url = $"http://ip-api.com/json/{ipAddress}?fields=status,lat,lon,countryCode";
            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<IpApiResponse>(cancellationToken: cancellationToken);
            if (json?.Status != "success" || json.Lat == null || json.Lon == null)
                return null;

            return new IpLocation(json.Lat.Value, json.Lon.Value, json.CountryCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IP geolocation lookup failed for {Ip}", ipAddress);
            return null;
        }
    }

    private static bool IsLocalOrPrivate(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip)) return true;

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168);
        }

        return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal;
    }

    private class IpApiResponse
    {
        public string? Status { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public string? CountryCode { get; set; }
    }
}
