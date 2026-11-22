
using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Infrastructure.Persistence.Configurations;

public class PropertyClaimDetailsConfiguration
    : IEntityTypeConfiguration<PropertyClaimDetails>
{
    public void Configure(
        EntityTypeBuilder<PropertyClaimDetails> builder)
    {
        builder.ToTable("PropertyClaimDetails");

        builder.HasKey(p => p.ClaimId);

        builder.Property(p => p.PropertyAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.PropertyType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.DamageDescription)
            .IsRequired()
            .HasMaxLength(2000);
    }
}
