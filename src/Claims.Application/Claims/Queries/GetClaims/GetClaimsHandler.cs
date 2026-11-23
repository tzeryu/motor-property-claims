using Claims.Application.Claims.DTOs.Queries;
using Claims.Application.Claims.Interfaces;
using Claims.Domain.Entities;
using Claims.Domain.Enums;

namespace Claims.Application.Claims.Queries.GetClaims;

public sealed class GetClaimsHandler
{
    private readonly IClaimRepository _claimRepository;

    public GetClaimsHandler(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }

    public async Task<PagedClaimsResult> GetForOwnerAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        ClaimStatus? status,
        CancellationToken cancellationToken = default)
    {
        ValidatePaging(ownerUserId, pageNumber, pageSize);

        var (claims, totalCount) =
            await _claimRepository.GetForOwnerAsync(
                ownerUserId,
                status,
                pageNumber,
                pageSize,
                cancellationToken
            );

        return CreateResult(claims, pageNumber, pageSize, totalCount);
    }

    public async Task<PagedClaimsResult> GetForStaffAsync(
        int pageNumber,
        int pageSize,
        ClaimStatus? status,
        Guid? assignedOfficerUserId,
        bool unassignedOnly,
        CancellationToken cancellationToken = default)
    {
        ValidatePaging(pageNumber, pageSize);

        var (claims, totalCount) =
            await _claimRepository.GetForStaffAsync(
                status,
                assignedOfficerUserId,
                unassignedOnly,
                pageNumber,
                pageSize,
                cancellationToken
            );

        return CreateResult(claims, pageNumber, pageSize, totalCount);
    }

    private static PagedClaimsResult CreateResult(
        IReadOnlyList<Claim> claims,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var items = claims.Select(c =>
            new ClaimListItemResult(
                c.Id,
                c.ClaimNumber,
                c.ClaimType,
                c.Status,
                c.ClaimantName,
                c.PolicyDetails.PolicyNumber,
                c.IncidentDate,
                c.SubmittedAtUtc,
                c.AssignedOfficerUserId,
                c.AssignedAtUtc
            )
        ).ToList();

        return new PagedClaimsResult(
            items,
            pageNumber,
            pageSize,
            totalCount
        );
    }

    private static void ValidatePaging(
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1 ||
            pageSize < 1 ||
            pageSize > 100 ||
            (long)(pageNumber - 1) * pageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Invalid pagination parameters."
            );
        }
    }

    private static void ValidatePaging(
        Guid ownerUserId,
        int pageNumber,
        int pageSize)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid owner user ID is required.",
                nameof(ownerUserId)
            );
        }

        ValidatePaging(pageNumber, pageSize);
    }
}