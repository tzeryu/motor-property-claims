
using Claims.Application.Claims.Interfaces;
using Claims.Domain.Entities;
using Claims.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Claims.Domain.Enums;
using Claims.Application.Claims.DTOs.Queries;


namespace Claims.Infrastructure.Repositories;

public sealed class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _dbContext;

    public ClaimRepository(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<Claim> Items, int TotalCount)>
    GetForOwnerAsync(
        Guid ownerUserId,
        ClaimStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Claims
            .AsNoTracking()
            .Where(c => c.SubmittedByUserId == ownerUserId);

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        return await GetPagedAsync(
            query,
            pageNumber,
            pageSize,
            cancellationToken
        );
    }

    public async Task<(IReadOnlyList<Claim> Items, int TotalCount)>
    GetForStaffAsync(
        ClaimStatus? status,
        Guid? assignedOfficerUserId,
        bool unassignedOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Claims.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (assignedOfficerUserId.HasValue)
        {
            query = query.Where(
                c => c.AssignedOfficerUserId == assignedOfficerUserId.Value
            );
        }
        else if (unassignedOnly)
        {
            query = query.Where(c => c.AssignedOfficerUserId == null);
        }

        return await GetPagedAsync(
            query,
            pageNumber,
            pageSize,
            cancellationToken
        );
    }

    private static async Task<(
        IReadOnlyList<Claim> Items,
        int TotalCount
    )> GetPagedAsync(
        IQueryable<Claim> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.SubmittedAtUtc)
            .ThenBy(c => c.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(
        Claim claim,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(claim);

        await _dbContext.Claims.AddAsync(
            claim,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Claim?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Claims
            .AsNoTracking()
            .Include(c => c.MotorDetails)
            .Include(c => c.PropertyDetails)
            .SingleOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);
    }

    public async Task<Claim?> GetByIdForOwnerAsync(
    Guid id,
    Guid ownerUserId,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims
            .AsNoTracking()
            .Include(c => c.MotorDetails)
            .Include(c => c.PropertyDetails)
            .SingleOrDefaultAsync(
                c => c.Id == id &&
                     c.SubmittedByUserId == ownerUserId,
                cancellationToken);
    }

    public async Task<Claim?> GetByIdForUpdateAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims
            .SingleOrDefaultAsync(
                c => c.Id == id,
                cancellationToken
            );
    }

    public async Task<Claim?> GetByIdForOwnerForUpdateAsync(
        Guid id,
        Guid ownerUserId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Claims
            .Where(c =>
                c.Id == id &&
                c.SubmittedByUserId == ownerUserId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ManagementOverviewResult> GetManagementOverviewAsync(
    CancellationToken cancellationToken = default)
    {
        // Query 1: Count claims by their current status.
        var statusCounts = await _dbContext.Claims
            .AsNoTracking()
            .GroupBy(c => c.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.Status,
                item => item.Count,
                cancellationToken
            );

        int Count(ClaimStatus status) =>
            statusCounts.GetValueOrDefault(status);

        var submitted = Count(ClaimStatus.Submitted);
        var inReview = Count(ClaimStatus.InReview);
        var awaitingInformation = Count(ClaimStatus.AwaitingInformation);
        var pendingManagerApproval = Count(ClaimStatus.PendingManagerApproval);
        var approvedUnpaid = Count(ClaimStatus.Approved);
        var settled = Count(ClaimStatus.Settled);
        var rejected = Count(ClaimStatus.Rejected);

        // Query 2: Calculate financial exposure separately by currency.
        // Query 2: Retrieve only the financial fields needed.
        // Grouping by the owned CurrencyCode property is handled in memory
        // because EF Core cannot translate the previous grouped projection.
        var financialRows = await _dbContext.Claims
            .AsNoTracking()
            .Where(c =>
                c.Status == ClaimStatus.PendingManagerApproval ||
                c.Status == ClaimStatus.Approved ||
                c.Status == ClaimStatus.Settled)
            .Select(c => new
            {
                CurrencyCode = c.PolicyDetails.CurrencyCode,
                c.Status,
                c.ProposedSettlementAmount,
                c.ApprovedSettlementAmount
            })
            .ToListAsync(cancellationToken);

        // Aggregate the small scalar projection by currency.
        var liabilityByCurrency = financialRows
            .GroupBy(c => c.CurrencyCode)
            .Select(group => new CurrencyExposureResult(
                group.Key,

                group
                    .Where(c => c.Status == ClaimStatus.PendingManagerApproval)
                    .Sum(c => c.ProposedSettlementAmount ?? 0m),

                group
                    .Where(c => c.Status == ClaimStatus.Approved)
                    .Sum(c => c.ApprovedSettlementAmount ?? 0m),

                group
                    .Where(c => c.Status == ClaimStatus.Settled)
                    .Sum(c => c.ApprovedSettlementAmount ?? 0m)
            ))
            .OrderBy(c => c.CurrencyCode)
            .ToList();

        var totalOutstanding =
            submitted +
            inReview +
            awaitingInformation +
            pendingManagerApproval +
            approvedUnpaid;

        return new ManagementOverviewResult(
            TotalOutstandingClaims: totalOutstanding,
            SubmittedClaims: submitted,
            InReviewClaims: inReview,
            AwaitingInformationClaims: awaitingInformation,
            PendingManagerApprovalClaims: pendingManagerApproval,
            ApprovedUnpaidClaims: approvedUnpaid,
            SettledClaims: settled,
            RejectedClaims: rejected,
            LiabilityByCurrency: liabilityByCurrency
        );
    }
}
