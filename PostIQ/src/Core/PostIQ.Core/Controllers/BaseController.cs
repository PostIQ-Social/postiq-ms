#nullable enable
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.Services;

namespace PostIQ.Core.Application.Controllers
{
    [ApiController]
    //[Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v1/[controller]")]
    public class BaseController : ControllerBase
    {
        private IMediator? _mediator;
        private IMapper? _mapper;
        private IIdentityService? _identity;
          
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
        protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetRequiredService<IMapper>();
        protected IIdentityService IdentityService => _identity ??= HttpContext.RequestServices.GetRequiredService<IIdentityService>();
        protected Task<IdentityDto?> Identity => IdentityService.GetIdentityAsync();

    }
}
 