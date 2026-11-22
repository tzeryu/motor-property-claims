namespace Claims.Application.Common.Authentication;

public sealed record GeneratedAccessToken(
    string Token,
    DateTimeOffset ExpiresAtUtc
);