namespace Claims.Infrastructure.Identity;

public sealed class DevelopmentUsersOptions
{
    public bool Enabled { get; set; }

    public DevelopmentUserSeed Claimant { get; set; } = new();

    public DevelopmentUserSeed ClaimsOfficer { get; set; } = new();

    public DevelopmentUserSeed Manager { get; set; } = new();
}

public sealed class DevelopmentUserSeed
{
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}