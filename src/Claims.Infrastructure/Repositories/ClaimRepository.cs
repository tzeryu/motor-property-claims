
using Claims.Application.Claims.Interfaces;
using Claims.Domain.Entities;
using Claims.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Claims.Domain.Enums;


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
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Claims.AsNoTracking();

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
}
