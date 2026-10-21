using System;

namespace HRMS.API.Helpers
{
    /// <summary>
    /// Provides consistent, cross-platform timezone resolution for AlienHR operations.
    /// Ensures that regardless of whether the API host runs on Windows, Linux containers,
    /// Render, Docker, or Azure (where system clock is typically UTC), business dates and
    /// attendance timestamps align precisely with Sri Lanka Standard Time (UTC+05:30).
    /// </summary>
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo SriLankaTimeZone = ResolveSriLankaTimeZone();

        private static TimeZoneInfo ResolveSriLankaTimeZone()
        {
            try
            {
                // Windows timezone identifier
                return TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
            }
            catch
            {
                try
                {
                    // Linux / macOS IANA identifier
                    return TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo");
                }
                catch
                {
                    // Universal fallback to UTC+05:30 offset
                    return TimeZoneInfo.CreateCustomTimeZone(
                        "SLST",
                        TimeSpan.FromMinutes(330),
                        "Sri Lanka Standard Time",
                        "Sri Lanka Standard Time");
                }
            }
        }

        /// <summary>
        /// Current business local time in Sri Lanka (UTC+05:30).
        /// </summary>
        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SriLankaTimeZone);

        /// <summary>
        /// Current business date in Sri Lanka.
        /// </summary>
        public static DateOnly Today => DateOnly.FromDateTime(Now);

        /// <summary>
        /// Converts any UTC timestamp to Sri Lanka local time.
        /// </summary>
        public static DateTime ToSriLankaTime(DateTime utcTime)
        {
            var utc = utcTime.Kind == DateTimeKind.Utc
                ? utcTime
                : DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, SriLankaTimeZone);
        }

        /// <summary>
        /// Gets the resolved Sri Lanka TimeZoneInfo.
        /// </summary>
        public static TimeZoneInfo TimeZone => SriLankaTimeZone;
    }
}
