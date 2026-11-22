using Claims.Domain.Enums;

namespace Claims.Application.Claims.DTOs.Queries;

public sealed record ClaimListItemResult(
    Guid Id,
    string ClaimNumber,
    ClaimType ClaimType,
    ClaimStatus Status,
    string ClaimantName,
    string PolicyNumber,
    DateOnly IncidentDate,
    DateTimeOffset SubmittedAtUtc
);