using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.BackgroundProcess.Configuration
{
    public class JobSettings
    {
        /// <summary>
        /// Whether the job is enabled. Default: true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Schedule cron expression (e.g. "0 */1 * * *" OR preset (e.g. "Min", "Hour", "Day", "Week", "2Weeks", "Month").
        /// If null/empty, job runs only on manual trigger.
        /// </summary>
        public string? Schedule { get; set; }

        /// <summary>
        /// Queue capacity (max items in channel). Default: 100
        /// </summary>
        public int QueueSize { get; set; } = 100;

        /// <summary>
        /// Max concurrent consumers processing items. Default: 5.
        /// </summary>
        public int MaxConcurrentConsumers { get; set; } = 5;

        /// <summary>
        /// Bounded channel full mode: Wait, DropUntilEnq, DropOldest, DropNewest. Default: Wait.
        /// </summary>
        public string FullMode { get; set; } = "Wait";

        /// <summary>
        /// When using Schedule as interval preset, minimum interval between producer runs in milliseconds. Default: 60000 (1 min)
        /// </summary>
        public int ProductionIntervalMs { get; set; } = 60000;

        /// <summary>
        /// Interval in milliseconds to set as NextExecutionTime after processing a job. Default: 60000 (1 min)
        /// </summary>
        public int NextExecutionIntervalMs { get; set; } = 60000;
    }
}
