using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.Application.Controllers;
using User.Application.Commands;

namespace User.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishController : BaseController
    {
        public PublishController() { }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdatePublished([FromBody] AddUpdatePublishedCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}
