using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PostIQ.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("reset-password")]
public sealed class PasswordResetPageController(IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    [Produces("text/html")]
    public IActionResult Get()
    {
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        return PhysicalFile(Path.Combine(webRoot, "reset-password.html"), "text/html; charset=utf-8");
    }
}
