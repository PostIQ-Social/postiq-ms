using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.Application.Controllers;
using Published.Application.Commands;
using User.Application.Commands;

namespace User.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishController : BaseController
    {
        public PublishController() { }

        [HttpPost]
        [Route("add-or-update")]
        public async Task<IActionResult> AddOrUpdatePublished([FromBody] AddUpdatePublishedCommand command)
        {
            var publish = await Mediator.Send(command);
            var jobCommand = new UpsertJobCommand
            {
                UserId = command.UserId,
                Source = command.Source,
                BaseUrl = command.BaseUrl,
                PublishedId = publish.Data,
            };
            var result = await Mediator.Send(jobCommand);
            return Ok(result);
        }
    }
}
