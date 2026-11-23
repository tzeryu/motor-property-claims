
using System.ComponentModel.DataAnnotations;
using Claims.Domain.Enums;

namespace Claims.Application.Claims.DTOs.Submission;

public sealed class SubmitClaimRequest : IValidatableObject
{
    // Claim classification
    [EnumDataType(typeof(ClaimType),
        ErrorMessage = "Claim type must be Motor or Property.")]
    public ClaimType ClaimType { get; init; }

    // Claimant information
    [Required]
    [StringLength(150)]
    public string? ClaimantName { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string? ClaimantEmail { get; init; }

    [Required]
    [Phone]
    [StringLength(30)]
    public string? ClaimantPhone { get; init; }

    // Policy information
    [Required(ErrorMessage = "Policy details are required.")]
    public PolicyDetailsRequest? PolicyDetails { get; init; }

    // Incident information
    [Required]
    public DateOnly? IncidentDate { get; init; }

    [Required]
    [StringLength(300)]
    public string? IncidentLocation { get; init; }

    [Required]
    [StringLength(2000)]
    public string? IncidentDescription { get; init; }

    // Motor-specific information
    public MotorClaimDetailsRequest? MotorDetails { get; init; }

    // Property-specific information
    public PropertyClaimDetailsRequest? PropertyDetails { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        // Validate claim type.
        if (!Enum.IsDefined(typeof(ClaimType), ClaimType))
        {
            yield return new ValidationResult(
                "A valid claim type must be supplied.",
                [nameof(ClaimType)]);

            yield break;
        }

        // Validate incident date.
        if (IncidentDate.HasValue &&
            IncidentDate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            yield return new ValidationResult(
                "Incident date cannot be in the future.",
                [nameof(IncidentDate)]);
        }

        if (PolicyDetails is not null &&
            (IncidentDate < PolicyDetails.CoverageStartDate ||
            IncidentDate > PolicyDetails.CoverageEndDate))
        {
            yield return new ValidationResult(
                "The incident date must fall within the policy coverage period.",
                new[]
                {
                    nameof(IncidentDate),
                    nameof(PolicyDetails)
                });
        }

        // Validate claim-type-specific details.
        switch (ClaimType)
        {
            case ClaimType.Motor:

                if (MotorDetails is null)
                {
                    yield return new ValidationResult(
                        "Motor details are required for a motor claim.",
                        [nameof(MotorDetails)]);
                }

                if (PropertyDetails is not null)
                {
                    yield return new ValidationResult(
                        "Property details must not be supplied for a motor claim.",
                        [nameof(PropertyDetails)]);
                }

                break;

            case ClaimType.Property:

                if (PropertyDetails is null)
                {
                    yield return new ValidationResult(
                        "Property details are required for a property claim.",
                        [nameof(PropertyDetails)]);
                }

                if (MotorDetails is not null)
                {
                    yield return new ValidationResult(
                        "Motor details must not be supplied for a property claim.",
                        [nameof(MotorDetails)]);
                }

                break;
        }
    }
}
