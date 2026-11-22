using Claims.Application.Common.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Claims.Infrastructure.Identity;

public sealed class IdentityUserAuthenticationService
    : IUserAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityUserAuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());

        if (user is null)
        {
            return null;
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            password,
            lockoutOnFailure: true
        );

        if (!signInResult.Succeeded)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthenticatedUser(
            UserId: user.Id,
            Email: user.Email ?? email,
            FullName: user.FullName,
            Roles: roles.ToArray()
        );
    }
}