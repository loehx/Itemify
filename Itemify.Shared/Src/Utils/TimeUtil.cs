
// ReSharper disable once CheckNamespace

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lustitia.Utils
{
    public static class TimeUtil
    {
        public static Func<DateTime, string> GetDateTimeFormatter(TimeSpan total, TimeSpan part)
        {
            throw new NotImplementedException();
        }

        public static string ToReadableString(this DateTime datetime, TimeSpan delta)
        {
            if (delta.TotalDays >= 1)
                return datetime.ToString("d");

            if (delta.Hours >= 1)
                return datetime.ToString("t");

            if (delta.Minutes >= 1)
                return datetime.ToString("t");

            if (delta.Seconds >= 1)
                return datetime.Second + " s";

            if (delta.Milliseconds >= 1)
                return datetime.Second + " ms";

            if (delta.Ticks >= 1)
                return datetime.Ticks + " tick";

            return "same";
        }

        [Obsolete("Please use ToReadableString instead.")]
        public static string ToShortDuration(this TimeSpan span, int chunkCount = 2, bool removeZeroes = true)
        {
            return ToReadableString(span, chunkCount, removeZeroes);
        }

        public static string ToReadableString(this TimeSpan span)
        {
            return ToReadableString(span, int.MaxValue, true);
        }


        public static string ToReadableString(this TimeSpan span, int chunkCount, bool removeZeroes)
        {
            // TODO: Change this to enumerable
            var formats = new[]
            {
                new { Unit = "y", Value = span.Days / 365 },
                new { Unit = "w", Value = (span.Days % 365) / 7 },
                new { Unit = "d", Value = span.Days % 365 % 7 },
                new { Unit = "h", Value = span.Hours },
                new { Unit = "m", Value = span.Minutes },
                new { Unit = "s", Value = span.Seconds },
                new { Unit = "ms", Value = span.Milliseconds },
                new { Unit = "t", Value = (int)(span.Ticks % TimeSpan.TicksPerMillisecond) }
            };

            var filtered = formats.SkipWhile(t => t.Value == 0)
                .Take(chunkCount)
                .Where(t => !removeZeroes || t.Value != 0)
                .Select(t => String.Format("{0}{1} ", t.Value, t.Unit));

            var result = String.Concat(filtered).Trim();
            if (string.IsNullOrEmpty(result))
                return "0h";

            return result;
        }


        [Obsolete("Please use ParseReadableString instead.")]
        public static TimeSpan ParseShortDuration(this string duration)
        {
            return ParseReadableString(duration);
        }

        public static TimeSpan ParseReadableString(string duration)
        {
            if (duration == null) throw new ArgumentNullException("duration");

            var formats = new[]
            {
                new { Unit = "ms", Ticks = TimeSpan.TicksPerMillisecond },
                new { Unit = "y", Ticks = TimeSpan.TicksPerSecond * 60 * 60 * 24 * 365 },
                new { Unit = "w", Ticks = TimeSpan.TicksPerSecond * 60 * 60 * 24 * 7 },
                new { Unit = "d", Ticks = TimeSpan.TicksPerSecond * 60 * 60 * 24 },
                new { Unit = "h", Ticks = TimeSpan.TicksPerSecond * 60 * 60 },
                new { Unit = "m", Ticks = TimeSpan.TicksPerSecond * 60 },
                new { Unit = "s", Ticks = TimeSpan.TicksPerSecond },
                new { Unit = "t", Ticks = 1L }
            };

            var parts = duration.Split(' ');
            long ticks = 0;

            foreach (var part in parts)
            {
                var format = formats.FirstOrDefault(k => part.EndsWith(k.Unit));
                if (format == null)
                    throw new Exception("Unknown unit in part: \"" + part + "\" of duration string: " + duration);

                var numberString = part.Substring(0, part.Length - format.Unit.Length);
                int number;
                if (!Int32.TryParse(numberString, out number))
                    throw new Exception("Cannot parse number in part: \"" + part + "\" of duration string: " + duration);

                ticks += format.Ticks * number;
            }

            return TimeSpan.FromTicks(ticks);
        }

        public static IEnumerable<DateTimeOffset> StepwiseTo(this DateTimeOffset start, DateTimeOffset end, TimeSpan stepSize)
        {
            while (start <= end)
            {
                yield return start;
                start += stepSize;
            }
        }

        public static DateTime Truncate(this DateTime source, DateTimeUnit unit)
        {
            switch (unit)
            {
                case DateTimeUnit.Ticks:
                    return source;
                case DateTimeUnit.Millisecond:
                    return source.Truncate(TimeSpan.TicksPerMillisecond);
                case DateTimeUnit.Second:
                    return source.Truncate(TimeSpan.TicksPerSecond);
                case DateTimeUnit.Minute:
                    return source.Truncate(TimeSpan.TicksPerMinute);
                case DateTimeUnit.Hour:
                    return source.Truncate(TimeSpan.TicksPerHour);
                case DateTimeUnit.Day:
                    return source.Date;
                case DateTimeUnit.Week:
                    return source.Date.AddDays(-(int)source.DayOfWeek);
                case DateTimeUnit.IsoWeek:
                    return source.Date.AddDays(-(int)source.GetIsoDayOfWeek());
                case DateTimeUnit.Month:
                    return new DateTime(source.Year, source.Month, 1);
                case DateTimeUnit.Quarter:
                    return new DateTime(source.Year, source.Month - (source.Month % 3), 1);
                case DateTimeUnit.Year:
                    return new DateTime(source.Year, 1, 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
            }
        }

        /// <summary>
        /// <para>Truncates a DateTime to a specified resolution.</para>
        /// <para>A convenient source for resolution is TimeSpan.TicksPerXXXX constants.</para>
        /// </summary>
        /// <param name="date">The DateTime object to truncate</param>
        /// <param name="resolution">e.g. to round to nearest second, TimeSpan.TicksPerSecond</param>
        /// <returns>Truncated DateTime</returns>
        public static DateTime Truncate(this DateTime date, long resolution)
        {
            return new DateTime(date.Ticks - (date.Ticks % resolution), date.Kind);
        }

        public static DateTime? ToUtcDateTime(this DateTimeOffset? value)
        {
            return value.HasValue ? value.Value.UtcDateTime : (DateTime?)null;
        }

        public static DateTimeOffset? ToDateTimeOffset(this DateTime? value)
        {
            return value.HasValue ? new DateTimeOffset?(value.Value) : null;
        }

        public static int DaysInYear(int year)
        {
            var thisYear = new DateTime(year, 1, 1);
            var nextYear = new DateTime(year + 1, 1, 1);

            return (nextYear - thisYear).Days;
        }
    }
}
