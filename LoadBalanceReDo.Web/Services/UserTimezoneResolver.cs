using GeoTimeZone;
using LoadBalanceReDo.Web.Models;

namespace LoadBalanceReDo.Web.Services;

public static class UserTimezoneResolver
{
    public static TimeZoneInfo? Resolve(IpLocation? location)
    {
        if (location == null) return null;

        try
        {
            var ianaId = TimeZoneLookup.GetTimeZone(location.Latitude, location.Longitude).Result;
            if (string.IsNullOrEmpty(ianaId)) return null;

            return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
        }
        catch
        {
            return null;
        }
    }
}
