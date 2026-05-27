using Microsoft.Extensions.Logging;
using PostIQ.Core.BackgroundProcess.Interfaces;

namespace PostIQ.Core.BackgroundProcess.Example
{
    public class ExampleJobProcessor : IJobItemProcessor<int>
    {
        private readonly ILogger<ExampleJobProducer> _logger;

        public ExampleJobProcessor(ILogger<ExampleJobProducer> logger)
        {
            _logger = logger;
        }
        public Task ProcessItemAsync(int item, CancellationToken cancellationToken = default)
        {
            // This is where you put the code to process one item (in this example, just an int).
            // In a real job, this might be an entity ID or a DTO with info needed to do the work.
            throw new NotImplementedException();
        }
    }
}
