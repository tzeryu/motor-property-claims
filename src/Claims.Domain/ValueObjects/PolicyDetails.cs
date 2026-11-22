
namespace Claims.Domain.ValueObjects;

public sealed class PolicyDetails
{
    private PolicyDetails()
    {
        // Required by Entity Framework Core.
    }

    public PolicyDetails(
        string policyNumber,
        string policyholderName,
        DateOnly coverageStartDate,
        DateOnly coverageEndDate,
        decimal insuredAmount,
        string currencyCode)
    {
        PolicyNumber = policyNumber;
        PolicyholderName = policyholderName;
        CoverageStartDate = coverageStartDate;
        CoverageEndDate = coverageEndDate;
        InsuredAmount = insuredAmount;
        CurrencyCode = currencyCode;
    }

    public string PolicyNumber { get; private set; } = string.Empty;

    public string PolicyholderName { get; private set; } = string.Empty;

    public DateOnly CoverageStartDate { get; private set; }

    public DateOnly CoverageEndDate { get; private set; }

    public decimal InsuredAmount { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;
}
