namespace LoadBalanceReDo.Web.Models;

public class OfficeDetailViewModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string City { get; init; }
    public required string Country { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public required string TimezoneId { get; init; }
    public required OfficeHoursResult Hours { get; init; }
}
