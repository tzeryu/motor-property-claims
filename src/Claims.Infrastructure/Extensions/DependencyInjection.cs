
using Claims.Application.Claims.Interfaces;
using Claims.Infrastructure.Identity;
using Claims.Infrastructure.Persistence;
using Claims.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Claims.Application.Common.Authentication;

namespace Claims.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Database configuration
        services.AddDbContext<ClaimsDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDataProtection();

        // ASP.NET Core Identity configuration
        services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;

        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddSignInManager()
    .AddEntityFrameworkStores<ClaimsDbContext>()
    .AddDefaultTokenProviders();

        // Repository implementations
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IdentitySeeder>();
        services.AddScoped<IUserAuthenticationService,
    IdentityUserAuthenticationService>();

        services.AddScoped<IAccessTokenGenerator,
            JwtAccessTokenGenerator>();


        return services;
    }
}
