using LoadBalanceReDo.Web.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;

namespace LoadBalanceReDo.Web.Services;

public class OfficeHoursService : IOfficeHoursService
{
    public OfficeHoursResult ResolveHours(Office office)
    {
        var officeTz = office.OfficeInfo?.GetTimeZoneInfo() ?? TimeZoneInfo.Utc;
        var nowUtc = DateTimeOffset.UtcNow;
        var nowInOffice = TimeZoneInfo.ConvertTime(nowUtc, officeTz);
        var todayLocal = DateOnly.FromDateTime(nowInOffice.DateTime);

        // 21-day window: Monday of last week through Sunday of next week
        var mondayThisWeek = todayLocal.AddDays(-((int)todayLocal.DayOfWeek + 6) % 7);
        if (todayLocal.DayOfWeek == DayOfWeek.Sunday)
            mondayThisWeek = todayLocal.AddDays(-6);
        var rangeStart = mondayThisWeek.AddDays(-7);
        var rangeEnd = mondayThisWeek.AddDays(20);

        var weeklyLookup = BuildWeeklyLookup(office.WeeklyHours);
        var overrides = BuildOverrideLookup(office.OfficeHolidays, rangeStart, rangeEnd);

        var days = new List<ResolvedDay>();
        for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
        {
            days.Add(ResolveDay(date, todayLocal, officeTz, weeklyLookup, overrides));
        }

        var todayEntry = days.FirstOrDefault(d => d.IsToday);
        var isOpen = todayEntry is { IsClosed: false, OpenUtc: not null, CloseUtc: not null }
                     && nowUtc >= todayEntry.OpenUtc && nowUtc < todayEntry.CloseUtc;

        TimeSpan? timeUntilClose = null;
        TimeSpan? timeUntilNextOpen = null;

        if (isOpen && todayEntry?.CloseUtc != null)
        {
            timeUntilClose = todayEntry.CloseUtc.Value - nowUtc;
        }
        else
        {
            timeUntilNextOpen = FindTimeUntilNextOpen(days, nowUtc);
        }

        return new OfficeHoursResult
        {
            Days = days,
            IsCurrentlyOpen = isOpen,
            NowUtc = nowUtc,
            TimeUntilClose = timeUntilClose,
            TimeUntilNextOpen = timeUntilNextOpen,
            OfficeTimezone = officeTz
        };
    }

    public OfficeCardViewModel BuildCard(Office office)
    {
        return new OfficeCardViewModel
        {
            Id = office.Key,
            Name = office.Name,
            Url = office.Url() ?? "#",
            City = office.OfficeInfo?.City ?? "",
            Country = office.OfficeInfo?.Country ?? "",
            Latitude = office.OfficeLocation?.Latitude,
            Longitude = office.OfficeLocation?.Longitude,
            Hours = ResolveHours(office)
        };
    }

    public OfficeDetailViewModel BuildDetail(Office office)
    {
        return new OfficeDetailViewModel
        {
            Id = office.Key,
            Name = office.Name,
            City = office.OfficeInfo?.City ?? "",
            Country = office.OfficeInfo?.Country ?? "",
            Latitude = office.OfficeLocation?.Latitude,
            Longitude = office.OfficeLocation?.Longitude,
            TimezoneId = office.OfficeInfo?.Timezone ?? "UTC",
            Hours = ResolveHours(office)
        };
    }

    private static DailyHoursEntry?[] BuildWeeklyLookup(IReadOnlyList<DailyHoursEntry>? weeklyHours)
    {
        var lookup = new DailyHoursEntry?[7];
        if (weeklyHours == null) return lookup;

        foreach (var entry in weeklyHours)
        {
            if (entry.Day >= 0 && entry.Day <= 6)
                lookup[entry.Day] = entry;
        }

        return lookup;
    }

    private static Dictionary<DateOnly, Holidays> BuildOverrideLookup(
        BlockListModel? holidays, DateOnly rangeStart, DateOnly rangeEnd)
    {
        var dict = new Dictionary<DateOnly, Holidays>();
        if (holidays == null) return dict;

        foreach (var block in holidays)
        {
            if (block.Content is not Holidays holiday || holiday.Date == null)
                continue;

            if (holiday.AnnuallyReoccurring)
            {
                var years = new HashSet<int> { rangeStart.Year, rangeEnd.Year };
                foreach (var year in years)
                {
                    if (holiday.Date.Value.Month == 2 && holiday.Date.Value.Day == 29
                        && !DateTime.IsLeapYear(year))
                        continue;

                    var projected = new DateOnly(year, holiday.Date.Value.Month, holiday.Date.Value.Day);
                    if (projected >= rangeStart && projected <= rangeEnd)
                    {
                        if (!dict.ContainsKey(projected))
                            dict[projected] = holiday;
                    }
                }
            }
            else
            {
                var date = holiday.Date.Value;
                if (date >= rangeStart && date <= rangeEnd)
                    dict[date] = holiday;
            }
        }

        return dict;
    }

    private static ResolvedDay ResolveDay(
        DateOnly date, DateOnly today, TimeZoneInfo officeTz,
        DailyHoursEntry?[] weeklyLookup,
        Dictionary<DateOnly, Holidays> overrides)
    {
        var dayOfWeek = date.DayOfWeek;
        var standard = weeklyLookup[(int)dayOfWeek];

        TimeOnly? openLocal;
        TimeOnly? closeLocal;
        bool isClosed;
        HoursSource source;

        if (overrides.TryGetValue(date, out var holiday))
        {
            source = HoursSource.Override;
            var adjustedOpen = holiday.AdjustedOpeningTime;
            var adjustedClose = holiday.AdjustedClosingTime;

            if (adjustedOpen == null && adjustedClose == null)
            {
                isClosed = true;
                openLocal = null;
                closeLocal = null;
            }
            else if (adjustedOpen != null && adjustedClose != null)
            {
                isClosed = false;
                openLocal = adjustedOpen;
                closeLocal = adjustedClose;
            }
            else if (adjustedOpen != null)
            {
                if (standard is { IsOpen: true } && standard.CloseTime != null)
                {
                    isClosed = false;
                    openLocal = adjustedOpen;
                    closeLocal = standard.CloseTime;
                }
                else
                {
                    isClosed = true;
                    openLocal = null;
                    closeLocal = null;
                }
            }
            else
            {
                if (standard is { IsOpen: true } && standard.OpenTime != null)
                {
                    isClosed = false;
                    openLocal = standard.OpenTime;
                    closeLocal = adjustedClose;
                }
                else
                {
                    isClosed = true;
                    openLocal = null;
                    closeLocal = null;
                }
            }
        }
        else
        {
            source = HoursSource.Standard;
            if (standard == null || !standard.IsOpen)
            {
                isClosed = true;
                openLocal = null;
                closeLocal = null;
            }
            else
            {
                isClosed = false;
                openLocal = standard.OpenTime;
                closeLocal = standard.CloseTime;
            }
        }

        return new ResolvedDay
        {
            Date = date,
            DayOfWeek = dayOfWeek,
            DayName = dayOfWeek.ToString(),
            IsClosed = isClosed,
            OpenUtc = LocalToUtc(date, openLocal, officeTz),
            CloseUtc = LocalToUtc(date, closeLocal, officeTz),
            Source = source,
            IsToday = date == today
        };
    }

    private static DateTimeOffset? LocalToUtc(DateOnly date, TimeOnly? time, TimeZoneInfo tz)
    {
        if (time == null) return null;

        var localDateTime = date.ToDateTime(time.Value, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, tz);
        return new DateTimeOffset(utc, TimeSpan.Zero);
    }

    private static TimeSpan? FindTimeUntilNextOpen(List<ResolvedDay> days, DateTimeOffset nowUtc)
    {
        foreach (var day in days)
        {
            if (day.IsClosed || day.OpenUtc == null) continue;
            if (day.OpenUtc <= nowUtc) continue;

            return day.OpenUtc.Value - nowUtc;
        }

        return null;
    }
}
