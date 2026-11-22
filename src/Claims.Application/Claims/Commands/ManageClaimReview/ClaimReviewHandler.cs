using Claims.Application.Claims.Interfaces;

namespace Claims.Application.Claims.Commands.ManageClaimReview;

public sealed class ClaimReviewHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly TimeProvider _timeProvider;

    public ClaimReviewHandler(
        IClaimRepository claimRepository,
        TimeProvider timeProvider)
    {
        _claimRepository = claimRepository;
        _timeProvider = timeProvider;
    }

    public async Task StartReviewAsync(
        Guid claimId,
        Guid reviewerUserId,
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

        claim.StartReview(
            reviewerUserId,
            _timeProvider.GetUtcNow()
        );

        await _claimRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RequestAdditionalInformationAsync(
        Guid claimId,
        Guid requesterUserId,
        string message,
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

        claim.RequestAdditionalInformation(
            requesterUserId,
            message,
            _timeProvider.GetUtcNow()
        );

        await _claimRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ProvideAdditionalInformationAsync(
        Guid claimId,
        Guid claimantUserId,
        string response,
        CancellationToken cancellationToken = default)
    {
        // Fetch only if this claim belongs to the authenticated claimant.
        var claim = await _claimRepository.GetByIdForOwnerForUpdateAsync(
            claimId,
            claimantUserId,
            cancellationToken
        );

        if (claim is null)
        {
            throw new KeyNotFoundException("Claim not found.");
        }

        claim.ProvideAdditionalInformation(
            claimantUserId,
            response,
            _timeProvider.GetUtcNow()
        );

        await _claimRepository.SaveChangesAsync(cancellationToken);
    }
}