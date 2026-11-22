
using Claims.Domain.Enums;

namespace Claims.Application.Claims.DTOs.Submission;

public sealed record SubmitClaimResult(
    Guid Id,
    string ClaimNumber,
    ClaimType ClaimType,
    ClaimStatus Status,
    DateTimeOffset SubmittedAtUtc
);
