using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.Application.Controllers;
using User.Application.Queries;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace User.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : BaseController
    {

        // GET api/<ProfileController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            GetUIserByIdQuery query = new GetUIserByIdQuery(id);
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{userId}/posts")]
        public async Task<IActionResult> GetPosts(long userId)
        {
            var query = new GetUserPostsQuery(userId);
            var result = await Mediator.Send(query);
            return Ok(result);
        }
    }
}
