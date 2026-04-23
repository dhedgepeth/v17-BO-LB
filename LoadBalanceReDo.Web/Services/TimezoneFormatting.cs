namespace LoadBalanceReDo.Web.Services;

public static class TimezoneFormatting
{
    public static string FormatOffset(TimeSpan offset)
    {
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        var abs = offset.Duration();
        return $"UTC{sign}{abs.Hours:D2}:{abs.Minutes:D2}";
    }
}
