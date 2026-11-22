
using Claims.Application.Claims.Commands.SubmitClaim;
using Claims.Application.Claims.DTOs.Queries;
using Claims.Application.Claims.DTOs.Submission;
using Claims.Application.Claims.Queries.GetClaimById;
using Microsoft.AspNetCore.Mvc;
using Claims.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Claims.Application.Claims.Queries.GetClaims;
using Claims.Domain.Enums;
using Claims.Application.Claims.Commands.ManageClaimReview;
using Claims.Application.Claims.DTOs.Workflow;
using Claims.Application.Claims.Commands.ManageClaimDecision;
using Claims.Application.Claims.Commands.ManageSettlement;



namespace Claims.Api.Controllers;

[ApiController]
[Route("api/v1/claims")]
public sealed class ClaimsController : ControllerBase
{
    private readonly SubmitClaimHandler _submitClaimHandler;
    private readonly GetClaimByIdHandler _getClaimByIdHandler;
    private readonly GetClaimsHandler _getClaimsHandler;
    private readonly ClaimReviewHandler _claimReviewHandler;
    private readonly ClaimDecisionHandler _claimDecisionHandler;
    private readonly SettlementHandler _settlementHandler;

    public ClaimsController(
    SubmitClaimHandler submitClaimHandler,
    GetClaimByIdHandler getClaimByIdHandler,
    GetClaimsHandler getClaimsHandler,
    ClaimReviewHandler claimReviewHandler,
    ClaimDecisionHandler claimDecisionHandler,
    SettlementHandler settlementHandler)
    {
        _submitClaimHandler = submitClaimHandler;
        _getClaimByIdHandler = getClaimByIdHandler;
        _getClaimsHandler = getClaimsHandler;
        _claimReviewHandler = claimReviewHandler;
        _claimDecisionHandler = claimDecisionHandler;
        _settlementHandler = settlementHandler;
    }

    // POST: api/v1/claims
    [Authorize(Roles = RoleNames.Claimant)]
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
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _submitClaimHandler.HandleAsync(
            request,
            userId,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    // GET: api/v1/claims/{id}
    [Authorize(
    Roles = RoleNames.ClaimsOfficer + "," + RoleNames.Manager
    )]
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

    [Authorize(Roles = RoleNames.Claimant)]
    [HttpGet("mine")]
    public async Task<ActionResult<PagedClaimsResult>> GetMyClaims(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] ClaimStatus? status = null,
    CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                Message = "Invalid pagination parameters."
            });
        }

        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _getClaimsHandler.GetForOwnerAsync(
            userId,
            pageNumber,
            pageSize,
            status,
            cancellationToken
        );

        return Ok(result);
    }

    [Authorize(
    Roles = RoleNames.ClaimsOfficer + "," + RoleNames.Manager
)]
    [HttpGet]
    public async Task<ActionResult<PagedClaimsResult>> GetAllClaims(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] ClaimStatus? status = null,
    CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                Message = "Invalid pagination parameters."
            });
        }

        var result = await _getClaimsHandler.GetForStaffAsync(
            pageNumber,
            pageSize,
            status,
            cancellationToken
        );

        return Ok(result);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var subject = User.FindFirst("sub")?.Value;

        return Guid.TryParse(subject, out userId)
            && userId != Guid.Empty;
    }

    [Authorize(Roles = RoleNames.ClaimsOfficer)]
    [HttpPost("{id:guid}/start-review")]
    public async Task<IActionResult> StartReview(
    Guid id,
    CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _claimReviewHandler.StartReviewAsync(
                id,
                userId,
                cancellationToken
            );

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Cannot start claim review.",
                Detail = ex.Message
            });
        }
    }

    [Authorize(Roles = RoleNames.ClaimsOfficer)]
    [HttpPost("{id:guid}/information-request")]
    public async Task<IActionResult> RequestAdditionalInformation(
    Guid id,
    [FromBody] RequestAdditionalInformationRequest request,
    CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _claimReviewHandler.RequestAdditionalInformationAsync(
                id,
                userId,
                request.Message,
                cancellationToken
            );

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Cannot request additional information.",
                Detail = ex.Message
            });
        }
    }

    [Authorize(Roles = RoleNames.Claimant)]
    [HttpPost("{id:guid}/information-response")]
    public async Task<IActionResult> ProvideAdditionalInformation(
    Guid id,
    [FromBody] ProvideAdditionalInformationRequest request,
    CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _claimReviewHandler.ProvideAdditionalInformationAsync(
                id,
                userId,
                request.Response,
                cancellationToken
            );

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Cannot provide additional information.",
                Detail = ex.Message
            });
        }
    }

    [Authorize(Roles = RoleNames.ClaimsOfficer)]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveClaim(
    Guid id,
    [FromBody] ApproveClaimRequest request,
    CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var status = await _claimDecisionHandler.ApproveByOfficerAsync(
                id,
                userId,
                request.SettlementAmount,
                cancellationToken
            );

            return Ok(new
            {
                Status = status,
                ManagerApprovalRequired =
                    status == ClaimStatus.PendingManagerApproval
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Claim cannot be approved.",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid approval request.",
                Detail = ex.Message
            });
        }
    }

    [Authorize(Roles = RoleNames.ClaimsOfficer)]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectClaim(
    Guid id,
    [FromBody] RejectClaimRequest request,
    CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _claimDecisionHandler.RejectByOfficerAsync(
                id,
                userId,
                request.Reason,
                cancellationToken
            );

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Claim cannot be rejected.",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid rejection request.",
                Detail = ex.Message
            });
        }
    }

    [Authorize(Roles = RoleNames.Manager)]
    [HttpPost("{id:guid}/manager-approve")]
    public async Task<IActionResult> ManagerApproveClaim(
    Guid id,
    CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _claimDecisionHandler.ApproveByManagerAsync(
                id,
                userId,
                cancellationToken
            );

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Manager approval cannot be completed.",
                Detail = ex.Message
            });
        }
    }

    [Authorize(Roles = RoleNames.Manager)]
    [HttpPost("{id:guid}/manager-reject")]
    public async Task<IActionResult> ManagerRejectClaim(
    Guid id,
    [FromBody] RejectClaimRequest request,
    CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _claimDecisionHandler.RejectByManagerAsync(
                id,
                userId,
                request.Reason,
                cancellationToken
            );

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Manager rejection cannot be completed.",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid rejection request.",
                Detail = ex.Message
            });
        }
    }

    [Authorize(
    Roles = RoleNames.ClaimsOfficer + "," + RoleNames.Manager
)]
    [HttpPost("{id:guid}/settlement")]
    public async Task<IActionResult> RecordSettlement(
    Guid id,
    CancellationToken cancellationToken)
    {
        try
        {
            await _settlementHandler.RecordSettlementAsync(
                id,
                cancellationToken
            );

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Settlement cannot be recorded.",
                Detail = ex.Message
            });
        }
    }

    [Authorize(
    Roles = RoleNames.ClaimsOfficer + "," + RoleNames.Manager
)]
    [HttpPut("{id:guid}/payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(
    Guid id,
    [FromBody] UpdatePaymentStatusRequest request,
    CancellationToken cancellationToken)
    {
        try
        {
            await _settlementHandler.UpdatePaymentStatusAsync(
                id,
                request.Status,
                request.PaymentReference,
                cancellationToken
            );

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Payment status cannot be updated.",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid payment information.",
                Detail = ex.Message
            });
        }
    }
}
