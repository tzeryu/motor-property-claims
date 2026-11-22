
using Claims.Application.Claims.Interfaces;
using Claims.Infrastructure.Identity;
using Claims.Infrastructure.Persistence;
using Claims.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;

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
                // User settings
                options.User.RequireUniqueEmail = true;

                // Password settings
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // Account lockout settings
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);

                options.Lockout.AllowedForNewUsers = true;

                // MVP: email confirmation is not yet implemented.
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ClaimsDbContext>()
            .AddDefaultTokenProviders();

        // Repository implementations
        services.AddScoped<IClaimRepository, ClaimRepository>();


        return services;
    }
}
