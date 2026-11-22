
using Claims.Application.Claims.Interfaces;
using Claims.Domain.Entities;
using Claims.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Repositories;

public sealed class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _dbContext;

    public ClaimRepository(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
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
}
