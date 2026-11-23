using Claims.Application.Claims.DTOs.Queries;
using Claims.Application.Claims.Interfaces;

namespace Claims.Application.Claims.Queries.GetManagementOverview;

public sealed class GetManagementOverviewHandler
{
    private readonly IClaimRepository _claimRepository;

    public GetManagementOverviewHandler(
        IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }

    public async Task<ManagementOverviewResult> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _claimRepository.GetManagementOverviewAsync(
            cancellationToken
        );
    }
}