namespace PostIQ.Core.BackgroundProcess.Configuration
{
    // Summary:
    // Configuration for background jobs (bind from "AppSettings:Jons.BackgroundJobs" section).
    // /Summary>
    public class BackgroundJobsConfiguration
    {
        public const string SectionName = "BackgroundJobs";
        // Summary:
        // Gets or sets settings keyed by job name.
        // /Summary>
        public Dictionary<string, JobSettings> Jobs { get; set; } = new();

        // Summary:
        // Gets default settings when a job has no entry in Jobs.
        // /Summary>
        public JobSettings Defaults { get; set; } = new();

        // Summary:
        // Merges settings for a job, merging with Default.
        // /Summary>
        public JobSettings GetJobSettings(string jobName)
        {
            if (Jobs != null && Jobs.TryGetValue(jobName, out var setting))
            {
                var d = Defaults;
                // Merge with default.
                return new JobSettings
                {
                    Enabled = setting.Enabled, // bool is non-nullable, so use value directly
                    Schedule = setting.Schedule ?? d.Schedule,
                    QueueSize = setting.QueueSize > 0 ? setting.QueueSize : d?.QueueSize ?? 100,
                    MaxConcurrentConsumers = setting.MaxConcurrentConsumers > 0 ? setting.MaxConcurrentConsumers : d?.MaxConcurrentConsumers ?? 5,
                    ProductionIntervalMs = setting.ProductionIntervalMs > 0 ? setting.ProductionIntervalMs : d?.ProductionIntervalMs ?? 60000,
                    FullMode = setting.FullMode ?? d.FullMode ?? "Wait",
                    NextExecutionIntervalMs = setting.NextExecutionIntervalMs > 0 ? setting.NextExecutionIntervalMs : d?.NextExecutionIntervalMs ?? 60000,
                };
            }

            // Return default settings.
            return Defaults ?? new JobSettings();
        }
    }
}