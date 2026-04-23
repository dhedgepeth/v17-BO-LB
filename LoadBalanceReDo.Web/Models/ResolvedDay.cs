namespace LoadBalanceReDo.Web.Models;

public class ResolvedDay
{
    // The date in the office's local timezone — used for the row label
    public required DateOnly Date { get; init; }
    public required DayOfWeek DayOfWeek { get; init; }
    public required string DayName { get; init; }
    public required bool IsClosed { get; init; }

    // UTC is the single source of truth. Convert to display timezone at the view layer.
    public DateTimeOffset? OpenUtc { get; init; }
    public DateTimeOffset? CloseUtc { get; init; }

    public required HoursSource Source { get; init; }
    public bool IsToday { get; init; }

    public TimeOnly? OpenIn(TimeZoneInfo tz)
        => OpenUtc.HasValue
            ? TimeOnly.FromDateTime(TimeZoneInfo.ConvertTime(OpenUtc.Value, tz).DateTime)
            : null;

    public TimeOnly? CloseIn(TimeZoneInfo tz)
        => CloseUtc.HasValue
            ? TimeOnly.FromDateTime(TimeZoneInfo.ConvertTime(CloseUtc.Value, tz).DateTime)
            : null;

    public DateOnly? DateIn(TimeZoneInfo tz)
        => OpenUtc.HasValue
            ? DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(OpenUtc.Value, tz).DateTime)
            : null;
}

public enum HoursSource
{
    Standard,
    Override
}
