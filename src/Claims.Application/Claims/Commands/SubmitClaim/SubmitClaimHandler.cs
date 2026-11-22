
using Claims.Application.Claims.DTOs.Submission;
using Claims.Application.Claims.Interfaces;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Domain.ValueObjects;

namespace Claims.Application.Claims.Commands.SubmitClaim;

public sealed class SubmitClaimHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly TimeProvider _timeProvider;

    public SubmitClaimHandler(
        IClaimRepository claimRepository,
        TimeProvider timeProvider)
    {
        _claimRepository = claimRepository;
        _timeProvider = timeProvider;
    }

    public async Task<SubmitClaimResult> HandleAsync(
    SubmitClaimRequest request,
    Guid submittedByUserId,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (submittedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid submitting user ID is required.",
                nameof(submittedByUserId)
            );
        }

        // 1. Generate submission timestamp.
        var submittedAtUtc = _timeProvider.GetUtcNow();

        // 2. Generate claim reference.
        var claimNumber = GenerateClaimNumber(submittedAtUtc);

        // 3. Convert policy request into a domain value object.
        var policyRequest = request.PolicyDetails
            ?? throw new ArgumentException(
                "Policy details are required.");

        var policyDetails = new PolicyDetails(
            Required(policyRequest.PolicyNumber, "Policy number"),
            Required(policyRequest.PolicyholderName, "Policyholder name"),
            policyRequest.CoverageStartDate
                ?? throw new ArgumentException("Coverage start date is required."),
            policyRequest.CoverageEndDate
                ?? throw new ArgumentException("Coverage end date is required."),
            policyRequest.InsuredAmount
                ?? throw new ArgumentException("Insured amount is required."),
            Required(policyRequest.CurrencyCode, "Currency code")
        );

        var claimantName = Required(request.ClaimantName, "Claimant name");
        var claimantEmail = Required(request.ClaimantEmail, "Claimant email");
        var claimantPhone = Required(request.ClaimantPhone, "Claimant phone");
        var incidentDate = request.IncidentDate
            ?? throw new ArgumentException("Incident date is required.");

        var incidentLocation = Required(
            request.IncidentLocation, "Incident location");

        var incidentDescription = Required(
            request.IncidentDescription, "Incident description");

        // 4. Create the correct claim entity.
        Claim claim;

        switch (request.ClaimType)
        {
            case ClaimType.Motor:

                var motorRequest = request.MotorDetails
                    ?? throw new ArgumentException(
                        "Motor details are required.");

                if (request.PropertyDetails is not null)
                {
                    throw new ArgumentException(
                        "Property details cannot be supplied for a motor claim.");
                }

                var motorDetails = new MotorClaimDetails(
                    Required(
                        motorRequest.VehicleRegistrationNumber,
                        "Vehicle registration number"),
                    Optional(motorRequest.VehicleMake),
                    Optional(motorRequest.VehicleModel)
                );

                claim = Claim.SubmitMotor(
                claimNumber,
                claimantName,
                claimantEmail,
                claimantPhone,
                submittedByUserId,
                policyDetails,
                incidentDate,
                incidentLocation,
                incidentDescription,
                motorDetails,
                submittedAtUtc
                );

                break;

            case ClaimType.Property:

                var propertyRequest = request.PropertyDetails
                    ?? throw new ArgumentException(
                        "Property details are required.");

                if (request.MotorDetails is not null)
                {
                    throw new ArgumentException(
                        "Motor details cannot be supplied for a property claim.");
                }

                var propertyDetails = new PropertyClaimDetails(
                    Required(
                        propertyRequest.PropertyAddress,
                        "Property address"),
                    Required(
                        propertyRequest.PropertyType,
                        "Property type"),
                    Required(
                        propertyRequest.DamageDescription,
                        "Damage description")
                );

                claim = Claim.SubmitProperty(
                    claimNumber,
                    claimantName,
                    claimantEmail,
                    claimantPhone,
                    submittedByUserId,
                    policyDetails,
                    incidentDate,
                    incidentLocation,
                    incidentDescription,
                    propertyDetails,
                    submittedAtUtc
                );

                break;

            default:

                throw new ArgumentOutOfRangeException(
                    nameof(request.ClaimType),
                    "Unsupported claim type.");
        }

        // 5. Persist the claim.
        await _claimRepository.AddAsync(
            claim,
            cancellationToken);

        // 6. Return submission result.
        return new SubmitClaimResult(
            claim.Id,
            claim.ClaimNumber,
            claim.ClaimType,
            claim.Status,
            claim.SubmittedAtUtc
        );
    }

    private static string GenerateClaimNumber(
        DateTimeOffset submittedAtUtc)
    {
        var suffix = Guid.NewGuid()
            .ToString("N")[..20]
            .ToUpperInvariant();

        return $"CLM-{submittedAtUtc:yyyy}-{suffix}";
    }

    private static string Required(
        string? value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static string? Optional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
