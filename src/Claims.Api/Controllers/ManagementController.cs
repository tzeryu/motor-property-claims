using Claims.Application.Claims.DTOs.Queries;
using Claims.Application.Claims.Queries.GetManagementOverview;
using Claims.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("api/v1/management")]
[Authorize(Roles = RoleNames.Manager)]
public sealed class ManagementController : ControllerBase
{
    private readonly GetManagementOverviewHandler _handler;

    public ManagementController(
        GetManagementOverviewHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("overview")]
    [ProducesResponseType(
        typeof(ManagementOverviewResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ManagementOverviewResult>> GetOverview(
        CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(cancellationToken);

        return Ok(result);
    }
}