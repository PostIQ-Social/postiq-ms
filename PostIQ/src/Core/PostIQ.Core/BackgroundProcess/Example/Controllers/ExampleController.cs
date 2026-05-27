using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.BackgroundProcess.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.BackgroundProcess.Example.Controllers
{
    public class ExampleController
    {
        private readonly IBackgroundJobTrigger _backgroundJobTrigger;

        public ExampleController(IBackgroundJobTrigger backgroundJobTrigger)
        {
            _backgroundJobTrigger = backgroundJobTrigger;
        }
        public async Task TriggerJob(string jobName, CancellationToken cancellationToken)
        {
            var found = await _backgroundJobTrigger.TriggerJobAsync(jobName, cancellationToken);
            if (!found)
            {
               // return NotFoundResult("")
            }
            else
            {
                //return AcceptedResult("job triggered");
            }
        }

        public async Task TriggerItemJob(string jobName, int itemId, CancellationToken cancellationToken)
        {
            var found = await _backgroundJobTrigger.TriggerJobItemAsync(jobName, itemId, cancellationToken);
            if (!found)
            {
                // return NotFoundResult("")
            }
            else
            {
                //return AcceptedResult("job triggered");
            }
        }
    }
}
