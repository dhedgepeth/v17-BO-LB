namespace LoadBalanceReDo.Web.Services;

public static class ClientIpResolver
{
    public static string? Resolve(HttpContext context, IConfiguration config, IWebHostEnvironment env)
    {
        // Dev-only override: query string ?testIp=... or config OfficeLocator:TestIpAddress
        if (!env.IsProduction())
        {
            var queryIp = context.Request.Query["testIp"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(queryIp)) return queryIp;

            var configIp = config["OfficeLocator:TestIpAddress"];
            if (!string.IsNullOrWhiteSpace(configIp)) return configIp;
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}
