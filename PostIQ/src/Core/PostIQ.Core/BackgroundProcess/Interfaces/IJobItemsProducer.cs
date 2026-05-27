using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.BackgroundProcess.Interfaces
{
    // <summary>
    /// Produces items for a job (e.g. fetch pending ids from DB). Used for scheduled and "run full job" manual trigger.
    /// </summary>
    /// <typeparam name="TItem">Type of item (e.g. int for id, or a DTO).</typeparam>
    public interface IJobItemsProducer<TItem>
    {
        /// <summary>
        /// Fetch items to process (e.g. pending report requests). Called on schedule or when job is triggered manually.
        /// </summary>
        /// <param name="maxItems">Max items to return (respect queue capacity).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<IReadOnlyList<TItem>> GetItemsToProcessAsync(int maxItems, CancellationToken cancellationToken = default);
    }
}
