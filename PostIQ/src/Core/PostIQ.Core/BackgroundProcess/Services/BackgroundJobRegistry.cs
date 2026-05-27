using PostIQ.Core.BackgroundProcess.Interfaces;
using System.Collections.Concurrent;

namespace PostIQ.Core.BackgroundProcess.Services
{
    /// <summary>
    /// Provides a thread-safe registry for managing background jobs and their associated trigger and status delegates
    /// by job name.
    /// </summary>
    /// <remarks>The BackgroundJobRegistry allows registration and retrieval of delegates for triggering jobs,
    /// triggering individual job items, and querying job queue depth. This class is intended for use in systems that
    /// schedule or monitor background processing tasks. All operations are safe for concurrent access from multiple
    /// threads.</remarks>
    public class BackgroundJobRegistry : IBackgroundJobRegistry
    {
        private readonly ConcurrentDictionary<string, (Func<CancellationToken, Task> TriggerJob, Func<object?, CancellationToken, Task> TriggerItem, Func<CancellationToken, Task<int>> GetQueueDepth)> _jobs = new();

        public void Register(
            string jobName,
            Func<CancellationToken, Task> triggerJobAsync,
            Func<object?, CancellationToken, Task> triggerItemAsync,
            Func<CancellationToken, Task<int>> getQueueDepthAsync)
        {
            _jobs[jobName] = (triggerJobAsync, triggerItemAsync, getQueueDepthAsync);
        }

        public bool TryGetTriggerJob(string jobName, out Func<CancellationToken, Task>? triggerJobAsync)
        {
            if (_jobs.TryGetValue(jobName, out var entry))
            {
                triggerJobAsync = entry.TriggerJob;
                return true;
            }

            triggerJobAsync = null;
            return false;
        }

        public bool TryGetTriggerItem(string jobName, out Func<object?, CancellationToken, Task>? triggerItemAsync)
        {
            if (_jobs.TryGetValue(jobName, out var entry))
            {
                triggerItemAsync = entry.TriggerItem;
                return true;
            }

            triggerItemAsync = null;
            return false;
        }

        public bool TryGetQueueDepth(string jobName, out Func<CancellationToken, Task<int>> getQueueDepthSync)
        {
            if (_jobs.TryGetValue(jobName, out var entry))
            {
                getQueueDepthSync = entry.GetQueueDepth;
                return true;
            }

            getQueueDepthSync = null;
            return false;
        }
    }
}
