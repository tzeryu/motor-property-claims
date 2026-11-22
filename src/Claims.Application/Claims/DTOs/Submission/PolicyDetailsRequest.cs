
using System.ComponentModel.DataAnnotations;

namespace Claims.Application.Claims.DTOs.Submission;

public sealed class PolicyDetailsRequest : IValidatableObject
{
    [Required(ErrorMessage = "Policy number is required.")]
    [StringLength(50)]
    public string? PolicyNumber { get; init; }

    [Required(ErrorMessage = "Policyholder name is required.")]
    [StringLength(150)]
    public string? PolicyholderName { get; init; }

    [Required(ErrorMessage = "Coverage start date is required.")]
    public DateOnly? CoverageStartDate { get; init; }

    [Required(ErrorMessage = "Coverage end date is required.")]
    public DateOnly? CoverageEndDate { get; init; }

    [Required]
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ErrorMessage = "Insured amount must be greater than zero.")]
    public decimal? InsuredAmount { get; init; }

    [Required]
    [RegularExpression("^[A-Z]{3}$",
        ErrorMessage = "Currency code must contain three uppercase letters.")]
    public string? CurrencyCode { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (CoverageStartDate.HasValue &&
            CoverageEndDate.HasValue &&
            CoverageStartDate.Value > CoverageEndDate.Value)
        {
            yield return new ValidationResult(
                "Coverage start date cannot be later than coverage end date.",
                [nameof(CoverageStartDate), nameof(CoverageEndDate)]);
        }
    }
}
