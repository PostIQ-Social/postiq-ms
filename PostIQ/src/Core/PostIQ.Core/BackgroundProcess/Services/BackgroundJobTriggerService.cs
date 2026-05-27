using PostIQ.Core.BackgroundProcess.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.BackgroundProcess.Services
{
    // <summary>
    /// Default implementation of IBackgroundJobTrigger that dispatches to jobs registered in IBackgroundJobRegistry.
    /// </summary>
    public class BackgroundJobTriggerService : IBackgroundJobTrigger
    {
        private readonly IBackgroundJobRegistry _registry;

        public BackgroundJobTriggerService(IBackgroundJobRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public async Task<bool> TriggerJobAsync(string jobName, CancellationToken cancellationToken = default)
        {
            if (_registry.TryGetTriggerJob(jobName, out var trigger) && trigger != null)
            {
                await trigger(cancellationToken);
                return true;
            }

            return false;
        }

        public async Task<bool> TriggerJobItemAsync<TItem>(string jobName, TItem item, CancellationToken cancellationToken = default)
        {
            if (_registry.TryGetTriggerItem(jobName, out var trigger) && trigger != null)
            {
                await trigger(item, cancellationToken);
                return true;
            }

            return false;
        }

        public async Task<int> GetQueueDepthAsync(string jobName, CancellationToken cancellationToken = default)
        {
            if (_registry.TryGetQueueDepth(jobName, out var getDepth) && getDepth != null)
            {
                return await getDepth(cancellationToken);
            }

            return -1;
        }
    }
}
