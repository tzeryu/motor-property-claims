
using Claims.Domain.Enums;
using Claims.Domain.ValueObjects;

namespace Claims.Domain.Entities;

public sealed class Claim
{
    private Claim()
    {
        // Required by Entity Framework Core.
    }

    private Claim(
        string claimNumber,
        ClaimType claimType,
        string claimantName,
        string claimantEmail,
        string claimantPhone,
        PolicyDetails policyDetails,
        DateOnly incidentDate,
        string incidentLocation,
        string incidentDescription,
        DateTimeOffset submittedAtUtc,
        MotorClaimDetails? motorDetails,
        PropertyClaimDetails? propertyDetails)
    {
        Id = Guid.NewGuid();

        ClaimNumber = claimNumber;
        ClaimType = claimType;
        Status = ClaimStatus.Submitted;

        ClaimantName = claimantName;
        ClaimantEmail = claimantEmail;
        ClaimantPhone = claimantPhone;

        PolicyDetails = policyDetails;

        IncidentDate = incidentDate;
        IncidentLocation = incidentLocation;
        IncidentDescription = incidentDescription;

        CreatedAtUtc = submittedAtUtc;
        SubmittedAtUtc = submittedAtUtc;

        MotorDetails = motorDetails;
        PropertyDetails = propertyDetails;
    }

    public Guid Id { get; private set; }

    public string ClaimNumber { get; private set; } = string.Empty;

    public ClaimType ClaimType { get; private set; }

    public ClaimStatus Status { get; private set; }

    // Claimant information
    public string ClaimantName { get; private set; } = string.Empty;

    public string ClaimantEmail { get; private set; } = string.Empty;

    public string ClaimantPhone { get; private set; } = string.Empty;

    // Policy information
    public PolicyDetails PolicyDetails { get; private set; } = null!;

    // Incident information
    public DateOnly IncidentDate { get; private set; }

    public string IncidentLocation { get; private set; } = string.Empty;

    public string IncidentDescription { get; private set; } = string.Empty;

    // Claim-specific information
    public MotorClaimDetails? MotorDetails { get; private set; }

    public PropertyClaimDetails? PropertyDetails { get; private set; }

    // Timestamps
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset SubmittedAtUtc { get; private set; }

    // Factory method for motor claims
    public static Claim SubmitMotor(
        string claimNumber,
        string claimantName,
        string claimantEmail,
        string claimantPhone,
        PolicyDetails policyDetails,
        DateOnly incidentDate,
        string incidentLocation,
        string incidentDescription,
        MotorClaimDetails motorDetails,
        DateTimeOffset submittedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(motorDetails);

        return new Claim(
            claimNumber,
            ClaimType.Motor,
            claimantName,
            claimantEmail,
            claimantPhone,
            policyDetails,
            incidentDate,
            incidentLocation,
            incidentDescription,
            submittedAtUtc,
            motorDetails,
            null);
    }

    // Factory method for property claims
    public static Claim SubmitProperty(
        string claimNumber,
        string claimantName,
        string claimantEmail,
        string claimantPhone,
        PolicyDetails policyDetails,
        DateOnly incidentDate,
        string incidentLocation,
        string incidentDescription,
        PropertyClaimDetails propertyDetails,
        DateTimeOffset submittedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(propertyDetails);

        return new Claim(
            claimNumber,
            ClaimType.Property,
            claimantName,
            claimantEmail,
            claimantPhone,
            policyDetails,
            incidentDate,
            incidentLocation,
            incidentDescription,
            submittedAtUtc,
            null,
            propertyDetails);
    }
}
