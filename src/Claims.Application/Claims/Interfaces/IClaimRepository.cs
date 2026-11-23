
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Application.Claims.DTOs.Queries;

namespace Claims.Application.Claims.Interfaces;

public interface IClaimRepository
{
    Task AddAsync(
        Claim claim,
        CancellationToken cancellationToken);

    Task<Claim?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Claim?> GetByIdForOwnerAsync(
    Guid id,
    Guid ownerUserId,
    CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Claim> Items, int TotalCount)> GetForOwnerAsync(
    Guid ownerUserId,
    ClaimStatus? status,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Claim> Items, int TotalCount)> GetForStaffAsync(
    ClaimStatus? status,
    Guid? assignedOfficerUserId,
    bool unassignedOnly,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default);

    Task<Claim?> GetByIdForUpdateAsync(
    Guid id,
    CancellationToken cancellationToken = default);

    Task<Claim?> GetByIdForOwnerForUpdateAsync(
        Guid id,
        Guid ownerUserId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<ManagementOverviewResult> GetManagementOverviewAsync(
    CancellationToken cancellationToken = default);
}
