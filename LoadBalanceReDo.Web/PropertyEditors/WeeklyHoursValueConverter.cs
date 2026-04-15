using System.Text.Json;
using LoadBalanceReDo.Web.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace LoadBalanceReDo.Web.PropertyEditors;

public class WeeklyHoursValueConverter : IPropertyValueConverter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorUiAlias == "WeeklyHours.PropertyEditorUi";

    public Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IReadOnlyList<DailyHoursEntry>);

    public PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public bool? IsValue(object? value, PropertyValueLevel level)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    public object? ConvertSourceToIntermediate(
        IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString();

    public object? ConvertIntermediateToObject(
        IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel,
        object? inter, bool preview)
    {
        if (inter is not string json || string.IsNullOrWhiteSpace(json))
            return Array.Empty<DailyHoursEntry>();

        return JsonSerializer.Deserialize<List<DailyHoursEntry>>(json, JsonOptions)
               ?? new List<DailyHoursEntry>();
    }

    public object? ConvertIntermediateToXPath(
        IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel,
        object? inter, bool preview)
        => inter?.ToString();
}
