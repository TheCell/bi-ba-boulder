using System;

namespace Thecell.Bibaboulder.Common.Extensions;

public static class DateTimeExtensions
{
    // How to convert correctly UTC Time with AddMonths etc.:
    // https://stackoverflow.com/questions/18041613/are-adding-weeks-months-or-years-to-a-date-time-independent-from-time-zone
    public static DateTime ToLocalTime(this DateTime dateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZone);
    }

    public static DateTime ToUtcTime(this DateTime dateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        if (timeZone.IsInvalidTime(dateTime) && timeZone.IsDaylightSavingTime(dateTime.AddHours(1)))
        {
            dateTime = dateTime.AddHours(1);
        }
        return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
    }
}
