
using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(
        DbContextOptions<ClaimsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Claim> Claims { get; set; } = null!;

    public DbSet<MotorClaimDetails> MotorClaimDetails { get; set; }
        = null!;

    public DbSet<PropertyClaimDetails> PropertyClaimDetails { get; set; }
        = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ClaimsDbContext).Assembly);
    }
}
