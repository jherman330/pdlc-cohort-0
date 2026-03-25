using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Core.Authentication;
using Todo.Core.Common;

namespace Todo.Api.Controllers;

/// <summary>
/// RBAC probe: requires <see cref="Permissions.LicensesWrite"/> (e.g. Executive role does not have this).
/// Used to verify 403 behavior in integration tests and documentation.
/// </summary>
[Route("api/v1/rbac-probe")]
[ApiController]
[Authorize(Policy = Permissions.LicensesWrite)]
public class RbacProbeController : BaseController
{
    [HttpGet]
    public IActionResult Get() => Ok("licenses-write");
}
