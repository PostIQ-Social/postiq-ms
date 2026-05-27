using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.BackgroundProcess.Interfaces
{
    // Internal registry for job names to trigger delegates. Used by the trigger service to dispatch manual triggers.
    public interface IBackgroundJobRegistry
    {
        void Register(
            string jobName,
            Func<CancellationToken, Task> triggerJobAsync,
            Func<object?, CancellationToken, Task> triggerItemAsync,
            Func<CancellationToken, Task<int>> getQueueDepthSync);

        bool TryGetTriggerJob(string jobName, out Func<CancellationToken, Task> triggerJobAsync);
        bool TryGetTriggerItem(string jobName, out Func<object?, CancellationToken, Task> triggerItemAsync);
        bool TryGetQueueDepth(string jobName, out Func<CancellationToken, Task<int>> getQueueDepthSync);
    }
}
