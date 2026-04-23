using System.Text.Json;
using LoadBalanceReDo.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoadBalanceReDo.Web.ViewComponents;

public class OpenClosedStatusViewComponent : ViewComponent
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public IViewComponentResult Invoke(OfficeHoursResult hours, string size = "default")
    {
        // Emit UTC ISO strings in the payload so client-side JS can convert to any timezone
        var clientData = new
        {
            officeTimezone = hours.OfficeTimezoneId,
            isOpen = hours.IsCurrentlyOpen,
            days = hours.Days.Select(d => new
            {
                date = d.Date.ToString("yyyy-MM-dd"),
                openUtc = d.OpenUtc?.ToString("o"),
                closeUtc = d.CloseUtc?.ToString("o"),
                isClosed = d.IsClosed
            })
        };

        ViewBag.HoursJson = JsonSerializer.Serialize(clientData, JsonOptions);
        ViewBag.IsOpen = hours.IsCurrentlyOpen;
        ViewBag.TimezoneId = hours.OfficeTimezoneId;
        ViewBag.TimeUntilClose = hours.TimeUntilClose;
        ViewBag.TimeUntilNextOpen = hours.TimeUntilNextOpen;
        ViewBag.Size = size;

        return View();
    }
}
