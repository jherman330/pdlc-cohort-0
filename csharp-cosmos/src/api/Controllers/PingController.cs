using Microsoft.AspNetCore.Mvc;
using Todo.Core.Common;

namespace Todo.Api.Controllers;

/// <summary>
/// Minimal v1 controller to exercise the API versioning and BaseController pipeline.
/// Returns standardized response using base controller Ok&lt;T&gt; for consistent payload shape.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public class PingController : BaseController
{
    /// <summary>
    /// Returns 200 OK with { "data": "pong" } per standardized API response format.
    /// </summary>
    [HttpGet]
    public IActionResult Get() => Ok("pong");
     
}
