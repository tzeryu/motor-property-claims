namespace Claims.Application.Common.Authentication;

public sealed record AuthenticatedUser(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles
);