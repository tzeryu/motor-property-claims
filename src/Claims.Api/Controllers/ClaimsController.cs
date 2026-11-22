
using Claims.Application.Claims.Commands.SubmitClaim;
using Claims.Application.Claims.DTOs.Queries;
using Claims.Application.Claims.DTOs.Submission;
using Claims.Application.Claims.Queries.GetClaimById;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("api/v1/claims")]
public sealed class ClaimsController : ControllerBase
{
    private readonly SubmitClaimHandler _submitClaimHandler;
    private readonly GetClaimByIdHandler _getClaimByIdHandler;

    public ClaimsController(
        SubmitClaimHandler submitClaimHandler,
        GetClaimByIdHandler getClaimByIdHandler)
    {
        _submitClaimHandler = submitClaimHandler;
        _getClaimByIdHandler = getClaimByIdHandler;
    }

    // POST: api/v1/claims
    [HttpPost]
    [ProducesResponseType(
        typeof(SubmitClaimResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubmitClaimResult>> Submit(
        [FromBody] SubmitClaimRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _submitClaimHandler.HandleAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    // GET: api/v1/claims/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ClaimDetailsResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDetailsResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var claim = await _getClaimByIdHandler.HandleAsync(
            id,
            cancellationToken);

        if (claim is null)
        {
            return NotFound(new
            {
                Message = "Claim not found."
            });
        }

        return Ok(claim);
    }
}
