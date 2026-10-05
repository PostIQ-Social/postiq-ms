using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Published.Application.Queries;

namespace PostIQ.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {            
            return Ok(new { PageNo = 0, PageSize = 10 });
        }
    }
}
