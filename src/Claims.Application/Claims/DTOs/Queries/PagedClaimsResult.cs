namespace Claims.Application.Claims.DTOs.Queries;

public sealed record PagedClaimsResult(
    IReadOnlyList<ClaimListItemResult> Items,
    int PageNumber,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages =>
        (int)Math.Ceiling((double)TotalCount / PageSize);
}