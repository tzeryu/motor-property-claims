using Microsoft.AspNetCore.Identity;

namespace Claims.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
}