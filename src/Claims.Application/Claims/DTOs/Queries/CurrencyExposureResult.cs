namespace Claims.Application.Claims.DTOs.Queries;

public sealed record CurrencyExposureResult(
    string CurrencyCode,
    decimal PendingApprovalExposure,
    decimal ApprovedUnpaidLiability,
    decimal SettledAmount)
{
    public decimal TotalOutstandingExposure =>
        PendingApprovalExposure + ApprovedUnpaidLiability;
}