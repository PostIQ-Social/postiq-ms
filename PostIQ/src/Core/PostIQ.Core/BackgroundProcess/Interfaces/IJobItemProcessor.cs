using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.BackgroundProcess.Interfaces
{
    // <summary>
    /// Processes a single job item. Implement this (or inherit from BaseBackgroundJobProcessor) for each job type.
    /// </summary>
    /// <typeparam name="TItem">Type of item to process (eg, report request id, entity id).</typeparam>
    public interface IJobItemProcessor<TItem>
    {
        /// <summary>
        /// Process one item.
        /// </summary>
        Task ProcessItemAsync(TItem item, CancellationToken cancellationToken = default);
    }
}
