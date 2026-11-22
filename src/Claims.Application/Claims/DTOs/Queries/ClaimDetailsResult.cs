
using Claims.Domain.Enums;

namespace Claims.Application.Claims.DTOs.Queries;

public sealed record ClaimDetailsResult(
    Guid Id,
    string ClaimNumber,
    ClaimType ClaimType,
    ClaimStatus Status,
    string ClaimantName,
    string ClaimantEmail,
    string ClaimantPhone,
    PolicyDetailsResult PolicyDetails,
    DateOnly IncidentDate,
    string IncidentLocation,
    string IncidentDescription,
    MotorClaimDetailsResult? MotorDetails,
    PropertyClaimDetailsResult? PropertyDetails,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset SubmittedAtUtc
);

public sealed record PolicyDetailsResult(
    string PolicyNumber,
    string PolicyholderName,
    DateOnly CoverageStartDate,
    DateOnly CoverageEndDate,
    decimal InsuredAmount,
    string CurrencyCode
);

public sealed record MotorClaimDetailsResult(
    string VehicleRegistrationNumber,
    string? VehicleMake,
    string? VehicleModel
);

public sealed record PropertyClaimDetailsResult(
    string PropertyAddress,
    string PropertyType,
    string DamageDescription
);
