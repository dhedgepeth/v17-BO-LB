namespace LoadBalanceReDo.Web.Models;

public class OfficeCardViewModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string City { get; init; }
    public required string Country { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? DistanceKm { get; set; }
    public required OfficeHoursResult Hours { get; init; }
}
