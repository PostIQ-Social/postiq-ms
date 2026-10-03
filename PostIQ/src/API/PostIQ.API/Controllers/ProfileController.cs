using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostIQ.API.Contracts;
using PostIQ.Core.Application.Controllers;
using PostIQ.Core.BackgroundProcess.Interfaces;
using Published.Application.Commands;
using Published.Application.Queries;
using User.Application.Contracts;
using User.Application.Queries;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace User.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : BaseController
    {
        private readonly IBackgroundJobTrigger _backgroundJobTrigger;
        public ProfileController(IBackgroundJobTrigger backgroundJobTrigger)
        {
            _backgroundJobTrigger = backgroundJobTrigger;
        }

        // GET api/<ProfileController>/5
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var identity = await Identity;
            if(identity == null)
            {
                return Unauthorized();
            }
            GetUserDetailsByGuidQuery query = new GetUserDetailsByGuidQuery(identity.AuthId);
            var user = await Mediator.Send(query);
            if (user.Data is null)
            {
                return NotFound();
            }

            var postsQuery = new GetJobsByUserIdQuery(identity.UserId);
            var posts = await Mediator.Send(postsQuery);
            var response = new ProfileResponse
            {
                UserId = identity.UserId,
                FirstName = user.Data.FirstName,
                LastName = user.Data.LastName,
                ReferralCode = user.Data.ReferralCode,
                Email = identity.Email ?? string.Empty,
                Posts = posts
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(AddPostRequest request)
        {
            var identity = await Identity;
            if (identity == null)
            {
                return Unauthorized();
            }
            var command = new AddJobCommand
            {
                BaseUrl = request.BaseUrl,
                PublishedId = 0,
                UserId = identity.UserId,
                Source = request.Source
            };
            var result = await Mediator.Send(command);
            if (!result.IsValid)
            {
                var message = string.Join(" ", result.Errors.SelectMany(error => error.Value));
                return Conflict(new { message });
            }

            return Ok(result);
        }

        [HttpGet("my-posts")]
        [Authorize]
        public async Task<IActionResult> GetPosts(int pageno, int pagesize)
        {
            var identity = await Identity;
            if(identity == null)
            {
                return Unauthorized();
            }
            var result = await Mediator.Send(new GetPostByUserIdQuery
            {
                UserId = identity.UserId,
                PageNo = pageno,
                PageSize = pagesize
            });

            if (result.Data is { Count: > 0 })
            {
                var postIds = result.Data.Select(post => post.Id).ToArray();
                var likedPostIds = await Mediator.Send(new GetLikedPostIdsQuery(identity.UserId, postIds));
                var likedPostIdSet = likedPostIds.ToHashSet();
                foreach (var post in result.Data)
                {
                    post.IsLiked = likedPostIdSet.Contains(post.Id);
                }
            }

            return Ok(result);
        }

        [HttpGet("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var identity = await Identity;
            if (identity == null)
            {
                return Unauthorized();
            }
            GetUserDetailsByGuidQuery query = new GetUserDetailsByGuidQuery(identity.AuthId);
            var user = await Mediator.Send(query);
            if (user.Data is null)
            {
                return NotFound();
            }

            var job = await Mediator.Send(new GetJobForTriggerQuery(identity.UserId));
            await _backgroundJobTrigger.TriggerJobItemAsync("RepoJob", job.Data[0]);

            return Ok();
        }
    }
}
