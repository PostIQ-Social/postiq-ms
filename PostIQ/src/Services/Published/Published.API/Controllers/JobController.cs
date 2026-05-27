using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.Application.Controllers;
using Published.Application.Commands;

namespace Published.API.Controllers
{
    public class JobController : BaseController
    {
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Index(AddJobCommand command)
        {
            var result = Mediator.Send(command);
            return Ok(result);
        }
    }
}
