namespace LoadBalanceReDo.Web.Models;

public class OfficeHoursResult
{
    public required IReadOnlyList<ResolvedDay> Days { get; init; }
    public required bool IsCurrentlyOpen { get; init; }
    public required DateTimeOffset NowUtc { get; init; }
    public TimeSpan? TimeUntilClose { get; init; }
    public TimeSpan? TimeUntilNextOpen { get; init; }
    public required TimeZoneInfo OfficeTimezone { get; init; }

    public string OfficeTimezoneId => OfficeTimezone.Id;
    public DateTimeOffset NowInOfficeTimezone => TimeZoneInfo.ConvertTime(NowUtc, OfficeTimezone);
    public string FormatOffset(TimeZoneInfo tz) => Services.TimezoneFormatting.FormatOffset(tz.GetUtcOffset(NowUtc));
}
