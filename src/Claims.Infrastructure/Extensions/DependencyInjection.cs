
using Claims.Application.Claims.Interfaces;
using Claims.Infrastructure.Persistence;
using Claims.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        // Repository implementations
        services.AddScoped<IClaimRepository, ClaimRepository>();

        return services;
    }
}
