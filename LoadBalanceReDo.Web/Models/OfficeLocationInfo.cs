namespace LoadBalanceReDo.Web.Models;

public class OfficeLocationInfo
{
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Timezone { get; set; }
    public string? UtcOffset { get; set; }

    /// Returns the TimeZoneInfo for this office's IANA timezone,
    /// or null if the timezone is not set or unrecognized
    public TimeZoneInfo? GetTimeZoneInfo()
    {
        if (string.IsNullOrEmpty(Timezone)) return null;
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(Timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
    }
}
