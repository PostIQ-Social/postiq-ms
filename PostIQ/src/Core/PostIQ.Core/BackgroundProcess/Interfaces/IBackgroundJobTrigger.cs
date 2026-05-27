using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.BackgroundProcess.Interfaces
{
    /// <summary>
    /// Allows manual triggering of background jobs from API/controllers.
    /// </summary>
    public interface IBackgroundJobTrigger
    {
        /// <summary>
        /// trigger a job by name: runs the producer and enqueues all current items.
        /// </summary>
        /// <param name="jobName">unique job name (must match registration)</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>true if job was found and triggered.</returns>
        Task<bool> TriggerJobAsync(string jobName, CancellationToken cancellationToken = default);

        /// <summary>
        /// trigger processing of a specific item for a job (enqueues only this item).
        /// </summary>
        /// <typeparam name="TItem">Item type.</typeparam>
        /// <param name="jobName">unique job name.</param>
        /// <param name="item">the item to process.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>true if job was found and item was enqueued.</returns>
        Task<bool> TriggerJobItemAsync<TItem>(string jobName, TItem item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get current queue depth for a job (if supported).
        /// </summary>
        Task<int> GetQueueDepthAsync(string jobName, CancellationToken cancellationToken = default);
    }
}
