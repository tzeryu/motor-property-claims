
using Claims.Domain.Entities;

namespace Claims.Application.Claims.Interfaces;

public interface IClaimRepository
{
    Task AddAsync(
        Claim claim,
        CancellationToken cancellationToken);

    Task<Claim?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
