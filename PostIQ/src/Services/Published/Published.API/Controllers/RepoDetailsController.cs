using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.Application.Controllers;
using Published.Application.Commands;
using Published.Application.Queries;

namespace Published.API.Controllers
{
    public class RepoDetailsController : BaseController
    {
        public RepoDetailsController() { }

        [HttpGet("Batch")]
        public async Task<IActionResult> Batch(int LastId, int BatchSize)
        {
            var query = new GetBatchReposQuery
            {
                AfterId = LastId,
                BatchSize = BatchSize
            };
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("Job")]
        public async Task<IActionResult> UpsertJob([FromBody] UpsertJobCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}
