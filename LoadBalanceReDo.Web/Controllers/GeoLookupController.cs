using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using GeoTimeZone;
using Microsoft.AspNetCore.Mvc;

namespace LoadBalanceReDo.Web.Controllers;

[ApiController]
[Route("api/geolookup")]
public class GeoLookupController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeoLookupController> _logger;

    public GeoLookupController(IHttpClientFactory httpClientFactory, ILogger<GeoLookupController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve(
        [FromQuery] double lat,
        [FromQuery] double lng,
        CancellationToken cancellationToken)
    {
        try
        {
            // Timezone — offline lookup via GeoTimeZone, returns IANA ID
            var ianaTimezone = TimeZoneLookup.GetTimeZone(lat, lng).Result;

            // Verify the IANA ID is usable by .NET for UTC conversion
            TimeZoneInfo? tzInfo = null;
            try
            {
                tzInfo = TimeZoneInfo.FindSystemTimeZoneById(ianaTimezone);
            }
            catch (TimeZoneNotFoundException)
            {
                _logger.LogWarning("IANA timezone {Timezone} not found on this system", ianaTimezone);
            }

            // Reverse geocode — Nominatim (OpenStreetMap), free, no API key
            string? city = null;
            string? country = null;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("LoadBalanceReDo", "1.0"));

            // Use invariant culture to ensure decimals use dots, not commas
            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lngStr = lng.ToString(CultureInfo.InvariantCulture);
            var url = $"https://nominatim.openstreetmap.org/reverse?lat={latStr}&lon={lngStr}&format=jsonv2&accept-language=en";

            try
            {
                var response = await client.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                    if (json.TryGetProperty("address", out var address))
                    {
                        city = address.TryGetProperty("city", out var c) ? c.GetString()
                             : address.TryGetProperty("town", out var t) ? t.GetString()
                             : address.TryGetProperty("village", out var v) ? v.GetString()
                             : address.TryGetProperty("municipality", out var m) ? m.GetString()
                             : null;

                        country = address.TryGetProperty("country", out var co) ? co.GetString() : null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Reverse geocode failed for {Lat}, {Lng}", lat, lng);
            }

            return Ok(new
            {
                city,
                country,
                timezone = ianaTimezone,
                utcOffset = tzInfo?.GetUtcOffset(DateTimeOffset.UtcNow).ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GeoLookup failed for {Lat}, {Lng}", lat, lng);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
