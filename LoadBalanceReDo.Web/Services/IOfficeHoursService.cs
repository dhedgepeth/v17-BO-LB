using LoadBalanceReDo.Web.Models;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace LoadBalanceReDo.Web.Services;

public interface IOfficeHoursService
{
    OfficeHoursResult ResolveHours(Office office);
    OfficeCardViewModel BuildCard(Office office);
    OfficeDetailViewModel BuildDetail(Office office);
}
