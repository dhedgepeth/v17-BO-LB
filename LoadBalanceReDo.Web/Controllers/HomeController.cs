using LoadBalanceReDo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;

namespace LoadBalanceReDo.Web.Controllers;

public class HomeController : RenderController
{
    private readonly IOfficeHoursService _hoursService;
    private readonly IIpGeolocationService _ipGeolocation;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public HomeController(
        ILogger<HomeController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IOfficeHoursService hoursService,
        IIpGeolocationService ipGeolocation,
        IConfiguration config,
        IWebHostEnvironment env)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _hoursService = hoursService;
        _ipGeolocation = ipGeolocation;
        _config = config;
        _env = env;
    }

    public override IActionResult Index()
    {
        return IndexAsync().GetAwaiter().GetResult();
    }

    private async Task<IActionResult> IndexAsync()
    {
        var home = CurrentPage as Home;
        var offices = home?.Descendants<Office>() ?? Enumerable.Empty<Office>();

        var countryFilter = Request.Query["country"].FirstOrDefault();

        var ipAddress = ClientIpResolver.Resolve(HttpContext, _config, _env);
        var userLocation = await _ipGeolocation.GetLocationAsync(ipAddress, HttpContext.RequestAborted);
        var userTimezone = UserTimezoneResolver.Resolve(userLocation);

        var cards = offices
            .Select(o => _hoursService.BuildCard(o))
            .Where(c => string.IsNullOrEmpty(countryFilter)
                        || c.Country.Equals(countryFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (userLocation != null)
        {
            foreach (var card in cards)
            {
                if (card.Latitude.HasValue && card.Longitude.HasValue)
                {
                    card.DistanceKm = DistanceCalculator.HaversineKm(
                        userLocation.Latitude, userLocation.Longitude,
                        card.Latitude.Value, card.Longitude.Value);
                }
            }

            cards = cards
                .OrderBy(c => c.DistanceKm ?? double.MaxValue)
                .ThenBy(c => c.Name)
                .ToList();
        }
        else
        {
            cards = cards.OrderBy(c => c.Name).ToList();
        }

        var allCountries = offices
            .Select(o => o.OfficeInfo?.Country)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToList();

        ViewBag.OfficeCards = cards;
        ViewBag.Countries = allCountries;
        ViewBag.SelectedCountry = countryFilter;
        ViewBag.UserLocated = userLocation != null;
        ViewBag.Use12HourFormat = userLocation?.CountryCode == "US";
        ViewBag.UseImperialUnits = userLocation?.CountryCode == "US";
        ViewBag.UserTimezone = userTimezone;

        return CurrentTemplate(CurrentPage);
    }
}
