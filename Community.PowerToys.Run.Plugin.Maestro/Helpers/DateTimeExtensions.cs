using System;

namespace Community.PowerToys.Run.Plugin.Maestro.Helpers;

internal static class DateTimeExtensions
{
    private static class TimeAgo
    {
        public const string DayAgo = "{0} day ago";
        public const string DaysAgo = "{0} days ago";
        public const string HourAgo = "{0} hour ago";
        public const string HoursAgo = "{0} hours ago";
        public const string MinuteAgo = "{0} minute ago";
        public const string MinutesAgo = "{0} minutes ago";
        public const string MonthAgo = "{0} month ago";
        public const string MonthsAgo = "{0} months ago";
        public const string SecondAgo = "{0} second ago";
        public const string SecondsAgo = "{0} seconds ago";
        public const string YearAgo = "{0} year ago";
        public const string YearsAgo = "{0} years ago";
    }

    public static string ToTimeAgo(this DateTimeOffset dateTime)
    {
        var delay = DateTimeOffset.Now - dateTime;
        return delay.ToTimeAgo();
    }

    public static string ToTimeAgo(this TimeSpan delay)
    {
        const int MAX_SECONDS_FOR_JUST_NOW = 10;

        if (delay.Days > 365)
        {
            var years = Math.Round(decimal.Divide(delay.Days, 365));
            return string.Format(years == 1 ? TimeAgo.YearAgo : TimeAgo.YearsAgo, years);
        }

        if (delay.Days > 30)
        {
            var months = delay.Days / 30;
            if (delay.Days % 31 != 0)
            {
                months += 1;
            }

            return string.Format(months == 1 ? TimeAgo.MonthAgo : TimeAgo.MonthsAgo, months);
        }

        if (delay.Days > 0)
        {
            return string.Format(delay.Days == 1 ? TimeAgo.DayAgo : TimeAgo.DaysAgo, delay.Days);
        }

        if (delay.Hours > 0)
        {
            return string.Format(delay.Hours == 1 ? TimeAgo.HourAgo : TimeAgo.HoursAgo, delay.Hours);
        }

        if (delay.Minutes > 0)
        {
            return string.Format(delay.Minutes == 1 ? TimeAgo.MinuteAgo : TimeAgo.MinutesAgo, delay.Minutes);
        }

        if (delay.Seconds > MAX_SECONDS_FOR_JUST_NOW)
        {
            return string.Format(TimeAgo.SecondsAgo, delay.Seconds);
        }

        if (delay.Seconds <= MAX_SECONDS_FOR_JUST_NOW)
        {
            return string.Format(TimeAgo.SecondAgo, delay.Seconds);
        }

        throw new NotSupportedException("The DateTime object does not have a supported value.");
    }
}
