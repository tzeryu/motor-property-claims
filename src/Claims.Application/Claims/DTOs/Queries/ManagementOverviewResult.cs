namespace Claims.Application.Claims.DTOs.Queries;

public sealed record ManagementOverviewResult(
    int TotalOutstandingClaims,
    int SubmittedClaims,
    int InReviewClaims,
    int AwaitingInformationClaims,
    int PendingManagerApprovalClaims,
    int ApprovedUnpaidClaims,
    int SettledClaims,
    int RejectedClaims,
    IReadOnlyList<CurrencyExposureResult> LiabilityByCurrency
);