namespace Claims.Application.Common.Authentication;

public sealed record LoginResult(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles
);