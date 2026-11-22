using Claims.Application.Claims.Interfaces;
using Claims.Domain.Enums;

namespace Claims.Application.Claims.Commands.ManageSettlement;

public sealed class SettlementHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly TimeProvider _timeProvider;

    public SettlementHandler(
        IClaimRepository claimRepository,
        TimeProvider timeProvider)
    {
        _claimRepository = claimRepository;
        _timeProvider = timeProvider;
    }

    public async Task RecordSettlementAsync(
        Guid claimId,
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

        claim.RecordSettlement(_timeProvider.GetUtcNow());

        await _claimRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePaymentStatusAsync(
        Guid claimId,
        PaymentStatus status,
        string? paymentReference,
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

        claim.RecordPaymentOutcome(
            status,
            paymentReference,
            _timeProvider.GetUtcNow()
        );

        await _claimRepository.SaveChangesAsync(cancellationToken);
    }
}