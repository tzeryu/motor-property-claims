using Claims.Application.Common.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Claims.Infrastructure.Identity;

public sealed class IdentitySeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DevelopmentUsersOptions _options;

    public IdentitySeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<DevelopmentUsersOptions> options)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _options = options.Value;
    }

    public async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            RoleNames.Claimant,
            RoleNames.ClaimsOfficer,
            RoleNames.Manager
        };

        foreach (var roleName in roles)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await _roleManager.CreateAsync(
                new IdentityRole<Guid>(roleName)
            );

            EnsureSucceeded(result, $"Creating role '{roleName}'");
        }
    }

    public async Task SeedDevelopmentUsersAsync()
    {
        await SeedUserAsync(
            _options.Claimant,
            RoleNames.Claimant
        );

        await SeedUserAsync(
            _options.ClaimsOfficer,
            RoleNames.ClaimsOfficer
        );

        await SeedUserAsync(
            _options.Manager,
            RoleNames.Manager
        );
    }

    private async Task SeedUserAsync(
        DevelopmentUserSeed seed,
        string roleName)
    {
        if (string.IsNullOrWhiteSpace(seed.Email) ||
            string.IsNullOrWhiteSpace(seed.FullName) ||
            string.IsNullOrWhiteSpace(seed.Password))
        {
            throw new InvalidOperationException(
                $"Development user configuration for '{roleName}' is incomplete."
            );
        }

        var email = seed.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = seed.FullName.Trim(),
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            var createResult = await _userManager.CreateAsync(
                user,
                seed.Password
            );

            EnsureSucceeded(
                createResult,
                $"Creating development user '{email}'"
            );
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                roleName
            );

            EnsureSucceeded(
                roleResult,
                $"Assigning role '{roleName}' to '{email}'"
            );
        }
    }

    private static void EnsureSucceeded(
        IdentityResult result,
        string operation)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error =>
                    $"{error.Code}: {error.Description}")
            );

            throw new InvalidOperationException(
                $"{operation} failed. {errors}"
            );
        }
    }
}