using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.BackgroundProcess.Configuration
{
    // <summary>
    /// Supported schedule interval presets (can be used in configuration JSON)
    /// Use Cronexpression for full flexibility
    /// </summary>
    public static class ScheduleInterval
    {
        /// <summary>Every 10 sec</summary>
        public const string TenSec = "10sec";
        /// <summary>Every 1 minute</summary>
        public const string OneMinute = "1min";
        /// <summary>Every 5 minutes</summary>
        public const string FiveMinutes = "5min";
        /// <summary>Every 15 minutes</summary>
        public const string FifteenMinutes = "15min";
        /// <summary>Every 30 minutes</summary>
        public const string ThirtyMinutes = "30min";
        /// <summary>Every 1 hour</summary>
        public const string OneHour = "1hour";
        /// <summary>Every 1 day (midnight)</summary>
        public const string OneDay = "1day";
        /// <summary>Every 1 week (e.g. Sunday midnight)</summary>
        public const string OneWeek = "1week";
        /// <summary>Every 2 weeks</summary>
        public const string TwoWeeks = "2weeks";
        /// <summary>Every 1 month (1st of month)</summary>
        public const string OneMonth = "1month";

        /// <summary>
        /// Converts a preset string to a cron expression.
        /// </summary>
        public static string? ToCronExpression(string? interval)
        {
            if (string.IsNullOrWhiteSpace(interval)) return null;

            return interval.ToLowerInvariant() switch
            {
                "1min" => "* * * * *",        // every minute
                "5min" => "*/5 * * * *",      // every 5 minutes
                "15min" => "*/15 * * * *",     // every 15 minutes
                "30min" => "*/30 * * * *",     // every 30 minutes
                "1hour" => "0 * * * *",       // every hour at 00
                "1day" => "0 0 * * *",        // every day at midnight
                "1week" => "0 0 * * 0",       // every Sunday at midnight
                "2weeks" => "0 0 */14 * *",    // every 2 weeks (Sunday) approximate
                "1month" => "0 0 1 * *",      // 1st of every month at midnight
                _ => null
            };
        }

        /// <summary>
        /// Converts a preset string to a TimeSpan for fixed-interval scheduling.
        /// </summary>
        public static TimeSpan? ToTimeSpan(string? interval)
        {
            if (string.IsNullOrWhiteSpace(interval))
                return null;

            return interval.ToLowerInvariant() switch
            {
                "10sec" => TimeSpan.FromSeconds(10),
                "1min" => TimeSpan.FromMinutes(1),
                "5min" => TimeSpan.FromMinutes(5),
                "15min" => TimeSpan.FromMinutes(15),
                "30min" => TimeSpan.FromMinutes(30),
                "1hour" => TimeSpan.FromHours(1),
                "1day" => TimeSpan.FromDays(1),
                "1week" => TimeSpan.FromDays(7),
                "2weeks" => TimeSpan.FromDays(14),
                "1month" => TimeSpan.FromDays(30),
                _ => null
            };
        }
    }
}
