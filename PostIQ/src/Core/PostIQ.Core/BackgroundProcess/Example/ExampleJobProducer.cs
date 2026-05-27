using Microsoft.Extensions.Logging;
using PostIQ.Core.BackgroundProcess.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.BackgroundProcess.Example
{
    public class ExampleJobProducer : IJobItemsProducer<int>
    {
        private readonly ILogger<ExampleJobProducer> _logger;

        public ExampleJobProducer(ILogger<ExampleJobProducer> logger)
        {
            _logger = logger;
        }
        public Task<IReadOnlyList<int>> GetItemsToProcessAsync(int maxItems, CancellationToken cancellationToken = default)
        {
            //query from db for pending items, return list of ids to process (up to maxItems)
            return Task.FromResult<IReadOnlyList<int>>(new List<int> { 1, 2, 3 });
        }
    }
}
