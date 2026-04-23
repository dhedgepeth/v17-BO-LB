using LoadBalanceReDo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace LoadBalanceReDo.Web.Controllers;

public class OfficeController : RenderController
{
    private readonly IOfficeHoursService _hoursService;
    private readonly IIpGeolocationService _ipGeolocation;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public OfficeController(
        ILogger<OfficeController> logger,
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
        var office = CurrentPage as Office;
        if (office == null) return NotFound();

        var ipAddress = ClientIpResolver.Resolve(HttpContext, _config, _env);
        var userLocation = await _ipGeolocation.GetLocationAsync(ipAddress, HttpContext.RequestAborted);
        var userTimezone = UserTimezoneResolver.Resolve(userLocation);

        var detail = _hoursService.BuildDetail(office);

        ViewBag.OfficeDetail = detail;
        ViewBag.Use12HourFormat = userLocation?.CountryCode == "US";
        ViewBag.UserTimezone = userTimezone;

        return CurrentTemplate(CurrentPage);
    }
}
