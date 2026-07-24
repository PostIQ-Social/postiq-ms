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

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] Home.Application.Commands.CreatePostCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(long postId)
        {
            var query = new Home.Application.Queries.GetCommentsQuery(postId);
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(long id, [FromBody] Home.Application.Commands.LikePostCommand command)
        {
            if (id != command.PostId)
            {
                return BadRequest("PostId mismatch");
            }
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id}/comment")]
        public async Task<IActionResult> CommentPost(long id, [FromBody] Home.Application.Commands.CommentPostCommand command)
        {
            if (id != command.PostId)
            {
                return BadRequest("PostId mismatch");
            }
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("comment/{commentId}/like")]
        public async Task<IActionResult> LikeComment(long commentId, [FromBody] Home.Application.Commands.LikeCommentCommand command)
        {
            if (commentId != command.CommentId)
            {
                return BadRequest("CommentId mismatch");
            }
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{postId}/comment/{parentCommentId}/reply")]
        public async Task<IActionResult> ReplyToComment(long postId, long parentCommentId, [FromBody] Home.Application.Commands.ReplyToCommentCommand command)
        {
            if (postId != command.PostId || parentCommentId != command.ParentCommentId)
            {
                return BadRequest("PostId or ParentCommentId mismatch");
            }
            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}
