using Home.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostIQ.Core.Application.Controllers;
using PostIQ.Identity.Services;
using Published.Application.Commands;
using Published.Application.Queries;
using Published.Application.Response;
using User.Application.Queries;

namespace Home.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : BaseController
    {
        private readonly AuthService _authService;

        public HomeController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int pageNo, int pageSize)
        {
            var query = new GetPostQuery
            {
                PageNo = pageNo,
                PageSize = pageSize
            };
            var result = await Mediator.Send(query);
            await AddLikedStateAsync(result.Data);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] string searchBy = "all", [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "A search query is required." });
            }

            var result = await Mediator.Send(new SearchPostsQuery(query, searchBy, pageNo, pageSize));
            await AddPostCountsAsync(result.Data);
            await AddLikedStateAsync(result.Data);
            return Ok(result);
        }

        [HttpGet("GetPostsByEmail")]
        public async Task<IActionResult> GetPostsByEmail(
            [FromQuery] string email,
            [FromQuery] int pageNo = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "An email address is required." });
            }

            if (pageNo < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Page number must be positive and page size must be between 1 and 100." });
            }

            var authUser = await _authService.GetGuidByEmailAsync(email, cancellationToken);
            if (!authUser.Ok)
            {
                return authUser.Status == StatusCodes.Status404NotFound
                    ? NotFound()
                    : BadRequest(new { message = authUser.Error });
            }

            var user = await Mediator.Send(new GetUserDetailsByGuidQuery(authUser.Value), cancellationToken);
            if (user.Data is null)
            {
                return NotFound();
            }

            var result = await Mediator.Send(new GetPostByUserIdQuery
            {
                UserId = user.Data.UserId,
                PageNo = pageNo,
                PageSize = pageSize
            }, cancellationToken);

            await AddPostCountsAsync(result.Data);
            await AddLikedStateAsync(result.Data);
            return Ok(result);
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(long postId)
        {
            var identity = User.Identity?.IsAuthenticated == true ? await Identity : null;
            var query = new Published.Application.Queries.GetCommentsQuery(postId, identity?.UserId);
            var result = await Mediator.Send(query);
            if (result.Data is { Count: > 0 })
            {
                var userIds = result.Data.Select(comment => comment.UserId).Distinct().ToArray();
                var displayNames = await Mediator.Send(new GetUserDisplayNamesQuery(userIds));
                foreach (var comment in result.Data)
                {
                    comment.AuthorName = displayNames.GetValueOrDefault(comment.UserId, "PostIQ member");
                }
            }

            return Ok(result);
        }

        [HttpGet("{postId:long}")]
        public async Task<IActionResult> GetPost(long postId)
        {
            var post = await Mediator.Send(new GetPostByIdQuery(postId));
            if (post is null)
            {
                return NotFound();
            }

            await AddPostCountsAsync(new[] { post });
            await AddLikedStateAsync(new[] { post });
            return Ok(post);
        }

        private async Task AddLikedStateAsync(IReadOnlyCollection<BatchRepoRes> posts)
        {
            if (posts.Count == 0 || User.Identity?.IsAuthenticated != true) return;

            var identity = await Identity;
            if (identity is null) return;

            var postIds = posts.Select(post => post.Id).ToArray();
            var likedPostIds = await Mediator.Send(new GetLikedPostIdsQuery(identity.UserId, postIds));
            var likedPostIdSet = likedPostIds.ToHashSet();
            foreach (var post in posts)
            {
                post.IsLiked = likedPostIdSet.Contains(post.Id);
            }
        }

        private async Task AddPostCountsAsync(IReadOnlyCollection<BatchRepoRes> posts)
        {
            if (posts.Count == 0) return;

            var countResult = await Mediator.Send(new Published.Application.Queries.GetPostCountQuery(posts.Select(post => post.Id).ToArray()));
            foreach (var post in posts)
            {
                var counts = countResult.Data.FirstOrDefault(item => item.PostId == post.Id);
                if (counts is null) continue;
                post.LikeCount = counts.LikeCount;
                post.CommentCount = counts.CommentCount;
            }
        }

        [HttpPost("{id}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost(long id, [FromBody] LikePostCommand command)
        {
            if (id != command.PostId)
            {
                return BadRequest("PostId mismatch");
            }
            var identity = await Identity;
            if (identity is null) return Unauthorized();
            command.UserId = identity.UserId;

            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id}/comment")]
        [Authorize]
        public async Task<IActionResult> CommentPost(long id, [FromBody] CommentPostCommand command)
        {
            if (id != command.PostId)
            {
                return BadRequest("PostId mismatch");
            }
            var identity = await Identity;
            if (identity is null) return Unauthorized();
            command.UserId = identity.UserId;

            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("comment/{commentId}/like")]
        [Authorize]
        public async Task<IActionResult> LikeComment(long commentId, [FromBody] LikeCommentCommand command)
        {
            if (commentId != command.CommentId)
            {
                return BadRequest("CommentId mismatch");
            }
            var identity = await Identity;
            if (identity is null) return Unauthorized();
            command.UserId = identity.UserId;

            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{postId}/comment/{parentCommentId}/reply")]
        [Authorize]
        public async Task<IActionResult> ReplyToComment(long postId, long parentCommentId, [FromBody] ReplyToCommentCommand command)
        {
            if (postId != command.PostId || parentCommentId != command.ParentCommentId)
            {
                return BadRequest("PostId or ParentCommentId mismatch");
            }
            var identity = await Identity;
            if (identity is null) return Unauthorized();
            command.UserId = identity.UserId;

            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}
