using Home.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.Application.Controllers;

namespace Home.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Get(int pageNo, int pageSize)
        {
            var query = new GetPostsQuery(pageNo, pageSize);
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
    }
}
