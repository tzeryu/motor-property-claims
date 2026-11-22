using Claims.Application.Claims.Interfaces;
using Claims.Domain.Enums;

namespace Claims.Application.Claims.Commands.ManageClaimDecision;

public sealed class ClaimDecisionHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly TimeProvider _timeProvider;
    private readonly decimal _officerApprovalLimit;
    private readonly string _authorityCurrencyCode;

    public ClaimDecisionHandler(
        IClaimRepository claimRepository,
        TimeProvider timeProvider,
        decimal officerApprovalLimit,
        string authorityCurrencyCode)
    {
        _claimRepository = claimRepository;
        _timeProvider = timeProvider;
        _officerApprovalLimit = officerApprovalLimit;
        _authorityCurrencyCode = authorityCurrencyCode;
    }

    public async Task<ClaimStatus> ApproveByOfficerAsync(
        Guid claimId,
        Guid officerUserId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetByIdForUpdateAsync(
            claimId,
            cancellationToken
        );

        if (claim is null)
        {
            throw new KeyNotFoundException("Claim not found.");
        }

        claim.SubmitOfficerApproval(
            officerUserId,
            amount,
            _officerApprovalLimit,
            _authorityCurrencyCode,
            _timeProvider.GetUtcNow()
        );

        await _claimRepository.SaveChangesAsync(cancellationToken);

        return claim.Status;
    }

    public async Task RejectByOfficerAsync(
        Guid claimId,
        Guid officerUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetByIdForUpdateAsync(
            claimId,
            cancellationToken
        );

        if (claim is null)
        {
            throw new KeyNotFoundException("Claim not found.");
        }

        claim.RejectByClaimsOfficer(
            officerUserId,
            reason,
            _timeProvider.GetUtcNow()
        );

        await _claimRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveByManagerAsync(
        Guid claimId,
        Guid managerUserId,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetByIdForUpdateAsync(
            claimId,
            cancellationToken
        );

        if (claim is null)
        {
            throw new KeyNotFoundException("Claim not found.");
        }

        claim.ApproveByManager(
            managerUserId,
            _timeProvider.GetUtcNow()
        );

        await _claimRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectByManagerAsync(
        Guid claimId,
        Guid managerUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetByIdForUpdateAsync(
            claimId,
            cancellationToken
        );

        if (claim is null)
        {
            throw new KeyNotFoundException("Claim not found.");
        }

        claim.RejectByManager(
            managerUserId,
            reason,
            _timeProvider.GetUtcNow()
        );

        await _claimRepository.SaveChangesAsync(cancellationToken);
    }
}