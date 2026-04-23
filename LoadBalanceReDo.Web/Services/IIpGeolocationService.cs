using LoadBalanceReDo.Web.Models;

namespace LoadBalanceReDo.Web.Services;

public interface IIpGeolocationService
{
    Task<IpLocation?> GetLocationAsync(string? ipAddress, CancellationToken cancellationToken = default);
}
