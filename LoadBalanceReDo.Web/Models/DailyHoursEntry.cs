namespace LoadBalanceReDo.Web.Models;

public class DailyHoursEntry
{
    public int Day { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public string Open { get; set; } = "09:00";
    public string Close { get; set; } = "17:00";

    public TimeOnly? OpenTime => IsOpen && TimeOnly.TryParse(Open, out var t) ? t : null;
    public TimeOnly? CloseTime => IsOpen && TimeOnly.TryParse(Close, out var t) ? t : null;
}
